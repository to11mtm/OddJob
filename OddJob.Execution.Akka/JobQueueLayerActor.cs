using System;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class JobQueueLayerActor : ActorBase
    {
        public JobQueueLayerActor(IJobQueueManager jobQueueManager)
        {
            jobQueue = jobQueueManager;
        }
        public IJobQueueManager jobQueue { get; protected set; }

        /// <summary>
        /// Overridable Method to allow Handling of a Queue Failure.
        /// </summary>
        /// <param name="failedCommand">The Command that Failed.</param>
        /// <param name="ex">The Exception relating to Queue Failure.</param>
        protected virtual void OnQueueFailure(object failedCommand, Exception ex)
        {

        }
        protected override bool Receive(object message)
        {
            try
            {


                if (message is ShutDownQueues)
                {
                    Context.Sender.Tell(new QueueShutDown());
                }
                else if (message is GetJobs)
                {
                    var msg = (GetJobs) message;
                    Context.Sender.Tell(jobQueue.GetJobs(new[] {msg.QueueName}, msg.FetchSize));
                }
                else if (message is MarkJobInProgress)
                {
                    jobQueue.MarkJobInProgress(((MarkJobInProgress) message).JobId);
                }
                else if (message is MarkJobFailed)
                {
                    jobQueue.MarkJobFailed(((MarkJobFailed) message).JobId);
                }
                else if (message is MarkJobSuccess)
                {
                    jobQueue.MarkJobSuccess(((MarkJobSuccess) message).JobId);
                }
                else if (message is MarkJobInRetryAndIncrement)
                {
                    var msg = ((MarkJobInRetryAndIncrement) message);
                    jobQueue.MarkJobInRetryAndIncrement(msg.JobId, msg.LastAttempt);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    OnQueueFailure(message, ex);
                }
                finally
                {

                }
            }

            return true;

        }
        
    }
}

