using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class aJobQueueLayerActor : ReceiveActor
    {
        public aJobQueueLayerActor(IJobQueueManager jobQueueManager)
        {
            jobQueue = jobQueueManager;
            ReceiveAsync<GetJobs>(GetJobsAsync);
            ReceiveAsync<GetSpecificJob>(GetSpecificJobAsync);
            ReceiveAsync<MarkJobInProgress>(MarkJobInProgressAsync);
            ReceiveAsync<MarkJobSuccess>(MarkJobSuccessAsync);
            ReceiveAsync<MarkJobFailed>(MarkJobFailedAsync);
            ReceiveAsync<MarkJobInRetryAndIncrement>(MarkJobInRetryAndIncrementAsync);
            Receive<ShutDownQueues>(sdq=>QueueShutDown());
        }
        public IJobQueueManager jobQueue { get; protected set; }

        /// <summary>
        /// Overridable Method to allow Handling of a Queue Failure.
        /// </summary>
        /// <param name="failedCommand">The Command that Failed.</param>
        /// <param name="ex">The Exception relating to Queue Failure.</param>
        protected virtual void OnQueueFailure(object failedCommand, Exception ex)
        {
            Context.GetLogger().Error(ex,"Error On command {0}", failedCommand.GetType().ToString());
        }

        protected override void PreRestart(Exception reason, object message)
        {
            try
            {
                OnQueueFailure(message, reason);
            }
            catch
            {
                //Intentional
            }
            base.PreRestart(reason, message);
        }

        private async Task MarkJobInRetryAndIncrementAsync(MarkJobInRetryAndIncrement mjirai)
        {
            
          await  jobQueue.MarkJobInRetryAndIncrementAsync(mjirai.JobId, mjirai.LastAttempt);
        }

        private async Task MarkJobSuccessAsync(MarkJobSuccess mjs)
        {
            await jobQueue.MarkJobSuccessAsync(mjs.JobId);
        }

        private async Task MarkJobFailedAsync(MarkJobFailed mjf)
        {
            await jobQueue.MarkJobFailedAsync(mjf.JobId);
        }

        private async Task MarkJobInProgressAsync(MarkJobInProgress mjip)
        {
            await jobQueue.MarkJobInProgressAsync(mjip.JobId);
        }

        private static void QueueShutDown()
        {
            Context.Sender.Tell(new QueueShutDown());
        }

        private async Task GetSpecificJobAsync(GetSpecificJob j)
        {
            var jobId = j.JobId;
            var job = await  jobQueue.GetJobAsync(jobId, true);
            if (job != null)
            {
                Context.Sender.Tell(new ExecuteJobCommand(job));
            }
        }

        private async Task GetJobsAsync(GetJobs msg)
        {
            var jobs = await jobQueue.GetJobsAsync(new[] {msg.QueueName},
                msg.FetchSize, (q) => q.MostRecentDate);
            Context.Sender.Tell(new JobSweepResponse(jobs, msg.SweepGuid));
        }
    }
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
            Context.GetLogger().Error(ex,"Error On command {0}", failedCommand.GetType().ToString());
        }
        protected override bool Receive(object message)
        {
            try
            {
                if (message is ShutDownQueues)
                {
                    Context.Sender.Tell(new QueueShutDown());
                }
                else if (message is GetSpecificJob)
                {
                    var j = (message as GetSpecificJob);
                    var job = jobQueue.GetJob(j.JobId, true, true);
                    if (job != null)
                    {
                        Context.Sender.Tell(new ExecuteJobCommand(job));
                    }
                }
                else if (message is GetJobs)
                {
                    var msg = (GetJobs) message;
                    var jobs = jobQueue.GetJobs(new[] {msg.QueueName},
                        msg.FetchSize, (q) => q.MostRecentDate);
                    Context.Sender.Tell(new JobSweepResponse(jobs, msg.SweepGuid));
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

