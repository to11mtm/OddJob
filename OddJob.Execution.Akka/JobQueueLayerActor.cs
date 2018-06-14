using Akka.Actor;

namespace OddJob.Execution.Akka
{
    public class JobQueueLayerActor : ActorBase
    {
        public JobQueueLayerActor(IJobQueueManager jobQueueManager)
        {
            jobQueue = jobQueueManager;
        }
        public IJobQueueManager jobQueue { get; protected set; }
        protected override bool Receive(object message)
        {
            if (message is ShutDownQueues)
            {
                Context.Sender.Tell(new QueueShutDown());
            }
            else if (message is GetJobs)
            {
               Context.Sender.Tell(jobQueue.GetJobs(new[] { ((GetJobs)message).QueueName }));
            }
            else if (message is MarkJobInProgress)
            {
                jobQueue.MarkJobInProgress(((MarkJobInProgress)message).JobId);
            }
            else if (message is MarkJobFailed)
            {
                jobQueue.MarkJobFailed(((MarkJobFailed)message).JobId);
            }
            else if (message is MarkJobSuccess)
            {
                jobQueue.MarkJobSuccess(((MarkJobSuccess)message).JobId);
            }
            else if (message is MarkJobInRetryAndIncrement)
            {
                var msg = ((MarkJobInRetryAndIncrement)message);
                jobQueue.MarkJobInRetryAndIncrement(msg.JobId, msg.LastAttempt);
            }
            else
            {
                return false;
            }
            return true;
        }
        
    }
}

