using Akka.Actor;
using System.Linq;

namespace OddJob.Execution.Akka
{
    public class JobQueueCoordinator : ActorBase
    {
        public IActorRef WorkerRouterRef { get; protected set; }
        public IJobQueueManager jobQueue { get; protected set; }
        public string QueueName { get; protected set; }
        public JobQueueCoordinator(Props workerProps, IJobQueueManager jobQueueManager)
        {
            jobQueue = jobQueueManager;
            WorkerRouterRef = Context.ActorOf(workerProps);
        }
        protected override bool Receive(object message)
        {
            if (message is JobSweep)
            {
                var jobsToQueue = jobQueue.GetJobs(new[] { QueueName }).ToList();
                foreach (var job in jobsToQueue)
                {
                    WorkerRouterRef.Tell(new ExecuteJobRequest(job));
                    jobQueue.MarkJobInProgress(job.JobId);
                }
            }
            else if (message is JobSuceeded)
            {
                jobQueue.MarkJobSuccess(((JobSuceeded)message).JobData.JobId);
            }
            else if (message is JobFailed)
            {
                jobQueue.MarkJobFailed(((JobFailed)message).JobData.JobId);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
