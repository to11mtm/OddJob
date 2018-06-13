using Akka.Actor;
using System;

namespace OddJob.Execution.Akka
{
    public class JobWorkerActor : ActorBase
    {
        public JobExecutor _jobExecutor { get; protected set; }

        public JobWorkerActor(JobExecutor jobExecutor)
        {
            _jobExecutor = jobExecutor;
        }

        protected override bool Receive(object message)
        {
            if (message is ShutDownQueues)
            {
                Context.Sender.Tell(new QueueShutDown());
            }
            else if (message is ExecuteJobRequest)
            {
                RunJob(message as ExecuteJobRequest);
            }
            else
            {
                return false;
            }
            return true;
        }
        public void RunJob(ExecuteJobRequest request)
        {
            try
            {
                _jobExecutor.ExecuteJob(request.JobData);
                Context.Sender.Tell(new JobSuceeded(request.JobData));
            }
            catch(Exception ex)
            {
                Context.Sender.Tell(new JobFailed(request.JobData, ex));
            }
        }
    }
}
