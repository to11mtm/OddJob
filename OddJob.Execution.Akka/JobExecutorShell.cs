using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

[assembly: InternalsVisibleTo("GlutenFree.OddJob.Execution.Akka.Test")]
namespace GlutenFree.OddJob.Execution.Akka
{
    public abstract class BaseJobExecutorShell : IDisposable
    {
        protected ActorSystem _actorSystem { get; private set; }
        
        internal  Dictionary<string, IActorRef> coordinatorPool = new Dictionary<string, IActorRef>();
        internal  Dictionary<string, ICancelable> cancelPulsePool = new Dictionary<string, ICancelable>();
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
            _actorSystem = ActorSystem.Create("Oddjob-Akka", ConfigurationFactory.ParseString(hocon));
            
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
        public void StartJobQueue(string queueName, int numWorkers, int pulseDelayInSeconds, int firstPulseDelayInSeconds = 5, Expression<Func<JobLockData,object>> priorityExpresssion = null)
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
                            pulseDelayInSeconds, firstPulseDelayInSeconds, priExpr),
                        TimeSpan.FromSeconds(10))
                    .Result;
                if (result is Configured)
                {
                    coordinatorPool.Add(queueName, jobCoordinator);
                    var cancelToken = _actorSystem.Scheduler.ScheduleTellRepeatedlyCancelable(
                        (int) TimeSpan.FromSeconds(firstPulseDelayInSeconds).TotalMilliseconds,
                        (int) TimeSpan.FromSeconds(pulseDelayInSeconds).TotalMilliseconds, jobCoordinator,
                        new JobSweep(), null);
                    cancelPulsePool.Add(queueName, cancelToken);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected Result from Configuration Set!");
                }

            }
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
                cancelPulsePool[queueName].Cancel();
                var result =
                    coordinatorPool[queueName].Ask(new ShutDownQueues(), TimeSpan.FromSeconds(timeoutInSeconds))
                        .Result as QueueShutDown;
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
