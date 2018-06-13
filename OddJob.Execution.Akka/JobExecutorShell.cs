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
        IDependencyResolver dependencyResolver;
        private Dictionary<string, IActorRef> coordinatorPool;
        private Dictionary<string, ICancelable> cancelPulsePool;
        string hoconString
        {
            get
            {
                return string.Format(@"
my-custom-mailbox {
    mailbox-type : ""{0}.{1},{2}""
}", typeof(ShutdownPriorityMailbox).Namespace, typeof(ShutdownPriorityMailbox).Name, typeof(ShutdownPriorityMailbox).Assembly.GetName().Name);
            }
        }
        public JobExecutorShell()
        {
            var hocon = global::Akka.Configuration.ConfigurationFactory.Default().ToString() + Environment.NewLine + hoconString;
            _actorSystem = ActorSystem.Create("Oddjob-Akka", ConfigurationFactory.ParseString(hocon));
        }
        public void StartJobQueue(string queueName, int numWorkers, int pulseDelayInSeconds)
        {
            var workerProps = dependencyResolver.Create(typeof(JobWorkerActor)).WithRouter(new ConsistentHashingPool(numWorkers, (msg) =>
             {
                 return msg.GetHashCode();
             }));
            var jobQueueProps = dependencyResolver.Create(typeof(JobQueueLayerActor));
            var jobCoordinator = _actorSystem.ActorOf(Props.Create(() => new JobQueueCoordinator(workerProps, jobQueueProps, queueName, numWorkers)).WithMailbox("shutdown-priority-mailbox"), queueName);
            coordinatorPool.Add(queueName, jobCoordinator);
            var cancelToken = _actorSystem.Scheduler.ScheduleTellRepeatedlyCancelable((int)TimeSpan.FromSeconds(5).TotalMilliseconds, (int)TimeSpan.FromSeconds(pulseDelayInSeconds).TotalMilliseconds, jobCoordinator, new JobSweep(), null);
            cancelPulsePool.Add(queueName, cancelToken);
        }
        public void ShutDownQueue(string queueName, TimeSpan? timeout)
        {
            cancelPulsePool[queueName].Cancel();
            var result = coordinatorPool[queueName].Ask(new ShutDownQueues(), timeout).Result as QueueShutDown;
        }
        public void Dispose()
        {
            
        }
    }
}
