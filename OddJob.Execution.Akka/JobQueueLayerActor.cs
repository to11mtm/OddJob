using Akka.Actor;

namespace OddJob.Execution.Akka
{
    public class JobQueueLayerActor : ActorBase
    {
        public IJobQueueManager jobQueue { get; protected set; }
        protected override bool Receive(object message)
        {
            if (message is GetJobs)
            {
                jobQueue.GetJobs(new[] { ((GetJobs)message).QueueName });
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

