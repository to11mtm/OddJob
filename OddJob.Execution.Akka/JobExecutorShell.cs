using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Akka.Routing;

namespace OddJob.Execution.Akka
{
    public class JobExecutorShell : IDisposable
    {
        private ActorSystem _actorSystem;
        IDependencyResolver _dependencyResolver;
        private Dictionary<string, IActorRef> coordinatorPool = new Dictionary<string, IActorRef>();
        private Dictionary<string, ICancelable> cancelPulsePool = new Dictionary<string, ICancelable>();
        string hoconString
        {
            get
            {
                return string.Format(@"
shutdown-priority-mailbox {
    mailbox-type : ""{0}.{1},{2}""
}
", typeof(ShutdownPriorityMailbox).Namespace, typeof(ShutdownPriorityMailbox).Name, typeof(ShutdownPriorityMailbox).Assembly.GetName().Name);
            }
        }
        public JobExecutorShell(Func<ActorSystem,IDependencyResolver> dependencyResolverCreator)
        {
            
            var hocon = global::Akka.Configuration.ConfigurationFactory.Default().ToString() + Environment.NewLine + hoconString;
            _actorSystem = ActorSystem.Create("Oddjob-Akka", ConfigurationFactory.ParseString(hocon));
            _dependencyResolver = dependencyResolverCreator(_actorSystem);
        }
        /// <summary>
        /// Starts a Job Queue. If the Queue already exists, it will not be added.
        /// To reconfigure a queue, first shut it down, then call Start again.
        /// </summary>
        /// <param name="queueName">The name of the Queue</param>
        /// <param name="numWorkers">The number of Workers for the Queue</param>
        /// <param name="pulseDelayInSeconds">The Delay in seconds between 'pulses'.</param>
        public void StartJobQueue(string queueName, int numWorkers, int pulseDelayInSeconds)
        {
            if (coordinatorPool.ContainsKey(queueName) == false)
            {
                var workerProps = _dependencyResolver.Create(typeof(JobWorkerActor)).WithRouter(new RoundRobinPool(numWorkers));
                var jobQueueProps = _dependencyResolver.Create(typeof(JobQueueLayerActor));
                var jobCoordinator = _actorSystem.ActorOf(Props.Create(() => new JobQueueCoordinator(workerProps, jobQueueProps, queueName, numWorkers)).WithMailbox("shutdown-priority-mailbox"), queueName);
                coordinatorPool.Add(queueName, jobCoordinator);
                var cancelToken = _actorSystem.Scheduler.ScheduleTellRepeatedlyCancelable((int)TimeSpan.FromSeconds(5).TotalMilliseconds, (int)TimeSpan.FromSeconds(pulseDelayInSeconds).TotalMilliseconds, jobCoordinator, new JobSweep(), null);
                cancelPulsePool.Add(queueName, cancelToken);
            }
        }
        /// <summary>
        /// Shuts down a Queue.
        /// </summary>
        /// <param name="queueName">The Queue to shut down</param>
        /// <param name="timeout">How long to wait before giving up on waiting. After Timeout has passed, there are no guarantees whether jobs are still running!</param>
        public void ShutDownQueue(string queueName, TimeSpan? timeout)
        {
            cancelPulsePool[queueName].Cancel();
            var result = coordinatorPool[queueName].Ask(new ShutDownQueues(), timeout).Result as QueueShutDown;
            coordinatorPool[queueName].Tell(PoisonPill.Instance);
            cancelPulsePool.Remove(queueName);
            coordinatorPool.Remove(queueName);
        }
        public void Dispose()
        {
            _actorSystem.Terminate().RunSynchronously();
        }
    }
}
