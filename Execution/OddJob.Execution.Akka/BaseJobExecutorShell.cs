using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.Configuration;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

[assembly: InternalsVisibleTo("GlutenFree.OddJob.Execution.Akka.Test")]
namespace GlutenFree.OddJob.Execution.Akka
{
    public abstract class BaseJobExecutorShell : IDisposable
    {
        protected ActorSystem _actorSystem { get; private set; }
        
        protected internal Dictionary<string, IActorRef> coordinatorPool = new Dictionary<string, IActorRef>();
        protected Dictionary<string, ICancelable> cancelPulsePool = new Dictionary<string, ICancelable>();
        string mailboxString
        {
            get
            {
                return string.Format(@"shutdown-priority-mailbox {{
    mailbox-type : ""{0}""
}}
", typeof(ShutdownPriorityMailbox).AssemblyQualifiedName);
            }
        }

        protected string CustomHoconString
        {
            get { return ""; }
        }
        protected BaseJobExecutorShell(IExecutionEngineLoggerConfig loggerConfig)
        {
            string loggerConfigString = string.Empty;
            if (loggerConfig != null)
            {
                if (loggerConfig.LogLevel != null)
                {
                    loggerConfigString = loggerConfigString +
                                         string.Format("akka.loglevel = {0}", loggerConfig.LogLevel.ToUpper());
                }
                if (loggerConfig.LoggerTypeFactory != null)
                {
                    loggerConfigString = loggerConfigString + string.Format(@"
                    akka.loggers=[""{0}.{1}, {2}""]", loggerConfig.LoggerTypeFactory.Method.ReturnType.Namespace,
                                             loggerConfig.LoggerTypeFactory.Method.ReturnType.Name,
                                             loggerConfig.LoggerTypeFactory.Method.ReturnType.Assembly.GetName().Name);
                }
                
                    
            }

            var hocon = CustomHoconString + Environment.NewLine + 
                        mailboxString + Environment.NewLine + loggerConfigString;
            _actorSystem = ActorSystem.Create("Oddjob-Akka-"+ Guid.NewGuid(), ConfigurationFactory.ParseString(hocon));
            
        }
        
        

        /// <summary>
        /// Starts a Job Queue. If the Queue already exists, it will not be added.
        /// To reconfigure a queue, first shut it down, then call Start again.
        /// </summary>
        /// <param name="queueName">The name of the Queue</param>
        /// <param name="numWorkers">The number of Workers for the Queue</param>
        /// <param name="pulseDelayInSeconds">The Delay in seconds between 'pulses'.</param>
        /// <param name="firstPulseDelayInSeconds">The time to wait before the first pulse delay. Default is 5. Can use any value greater than 0</param>
        /// <param name="priorityExpresssion">An expression to use for setting retrieval priority. Default is most recent Attempt/Creation</param>
        /// <param name="aggressiveSweep">If true, when queues are saturated, a silent 'resweep' will be repeatedly sent for each saturated pulse, until the saturation condition has ended. These repeated attempts will not trigger the 'OnJobQueueSaturated' method or further increment the counters.</param>
        public void StartJobQueue(string queueName, int numWorkers, int pulseDelayInSeconds, int firstPulseDelayInSeconds = 5, Expression<Func<JobLockData,object>> priorityExpresssion = null, bool aggressiveSweep = false, int maxPendingFetches=30, int fetchTimeoutSeconds = 30, IEnumerable<IJobExecutionPluginConfiguration> plugins=null)
        {
            if (coordinatorPool.ContainsKey(queueName) == false)
            {

                var priExpr = priorityExpresssion;
                if (priExpr == null)
                {
                    priExpr = DefaultJobQueueManagerPriority.Expression;
                }

                var jobCoordinator = _actorSystem.ActorOf(CoordinatorProps.WithMailbox("shutdown-priority-mailbox"),
                    queueName);
                var result = jobCoordinator.Ask(new SetJobQueueConfiguration(WorkerProps, JobQueueProps, queueName,
                            numWorkers,
                            pulseDelayInSeconds, firstPulseDelayInSeconds, priExpr, aggressiveSweep, 0, maxPendingFetches, pendingSweepTimeoutSeconds:fetchTimeoutSeconds, executionPlugins: plugins),
                        TimeSpan.FromSeconds(10))
                    .Result;
                
                if (result is Configured)
                {
                    
                    coordinatorPool.Add(queueName, jobCoordinator);
                    BeforeQueueStart(queueName, jobCoordinator);
                    var cancelToken = _actorSystem.Scheduler.ScheduleTellRepeatedlyCancelable(
                        (int) TimeSpan.FromSeconds(firstPulseDelayInSeconds).TotalMilliseconds,
                        (int) TimeSpan.FromSeconds(pulseDelayInSeconds).TotalMilliseconds, jobCoordinator,
                        new JobSweep(), null);
                    cancelPulsePool.Add(queueName, cancelToken);
                    OnQueueStart(queueName, jobCoordinator,cancelToken);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected Result from Configuration Set!");
                }

            }
        }

        /// <summary>
        /// This allows for an extension point on your Coordinator, to perhaps set special configurations if needed after start,
        /// or for creating advanced pipeline scenarios.
        /// </summary>
        /// <param name="queueName">The name of the queue that is about to start.</param>
        /// <param name="coordinatorRef">A Thread-safe <see cref="IActorRef"/>that can be used to communicate with a custom coordinator.</param>
        /// <remarks>This may be useful for some special scenarios where you wish to set configurations before anything else is configured.</remarks>
        protected virtual void BeforeQueueStart(string queueName, IActorRef coordinatorRef)
        {

        }

        /// <summary>
        /// This allows for an extension point on your Coordinator, to perhaps set special configurations if needed after start,
        /// or for creating advanced pipeline scenarios.
        /// </summary>
        /// <param name="queueName">The name of the queue that has been started.</param>
        /// <param name="coordinatorRef">A Thread-safe <see cref="IActorRef"/>that can be used to communicate with a custom coordinator.</param>
        /// <param name="cancelToken">A Cancellation token for the initial worker</param>
        /// <remarks>
        /// This can be useful for highly-responsive scenarios. Consider where a SQL Server queue or RabbitMQ could be set to wait for messages,
        /// then telling the coordinator to sweep immediately. This can greatly simplify the complexities of working with such a bus.
        /// </remarks>
        protected virtual void OnQueueStart(string queueName, IActorRef coordinatorRef, ICancelable cancelToken)
        {

        }

        protected abstract Props WorkerProps { get; }
        protected abstract Props JobQueueProps { get; }
        protected abstract Props CoordinatorProps { get; }
        /// <summary>
        /// Shuts down a Queue.
        /// </summary>
        /// <param name="queueName">The Queue to shut down</param>
        /// <param name="timeoutInSeconds">How long to wait before giving up on waiting. After Timeout has passed, there are no guarantees whether jobs are still running, or whether a completed job will be marked as successful. Default is 2 minutes.</param>
        public void ShutDownQueue(string queueName, int timeoutInSeconds = 120)
        {
            try
            {
                if (cancelPulsePool.ContainsKey(queueName))
                {
                    if (cancelPulsePool[queueName].IsCancellationRequested == false)
                    {
                        cancelPulsePool[queueName].Cancel();
                    }
                }

                if (coordinatorPool.ContainsKey(queueName))
                {
                    var result =
                        coordinatorPool[queueName].Ask(new ShutDownQueues(), TimeSpan.FromSeconds(timeoutInSeconds))
                            .Result as QueueShutDown;
                }
                
                coordinatorPool[queueName].Tell(PoisonPill.Instance);
            }
            finally
            {
                try
                {
                    cancelPulsePool.Remove(queueName);
                }
                finally
                {

                }

                try
                {
                    coordinatorPool.Remove(queueName);
                }
                finally
                {

                }
            }
        }
        public void Dispose()
        {
            var keys = cancelPulsePool.Select(q => q.Key).ToList();
            foreach (var key in keys)
            {
                ShutDownQueue(key);
            }
            _actorSystem.Terminate().RunSynchronously();
        }
    }
}
