using System;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class JobWorkerActor : ActorBase
    {
        public IJobExecutor _jobExecutor { get; protected set; }

        public JobWorkerActor(IJobExecutor jobExecutor)
        {
            _jobExecutor = jobExecutor;
        }

        protected override bool Receive(object message)
        {
            if (message is ShutDownQueues)
            {
                //By design when we get this message it should mean the queue is drained.
                Context.Sender.Tell(new QueueShutDown());
            }
            else if (message is ExecuteJobRequest)
            {
                RunJob(message as ExecuteJobRequest);
            }
            else
            {
                //Unhandled.
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
