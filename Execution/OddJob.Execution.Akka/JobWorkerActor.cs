using System;
using System.Threading.Tasks;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class JobWorkerActor : ReceiveActor
    {
        public IJobExecutor _jobExecutor { get; protected set; }

        public JobWorkerActor(IJobExecutor jobExecutor)
        {
            _jobExecutor = jobExecutor;
            ReceiveAsync<object>(ReceiveMethod, message => 
                message is ExecuteJobRequest || message is ShutDownQueues);
        }

        protected async Task<bool> ReceiveMethod(object message)
        {
            if (message is ShutDownQueues)
            {
                //By design when we get this message it should mean the queue is drained.
                Context.Sender.Tell(new QueueShutDown());
            }
            else if (message is ExecuteJobRequest)
            {
                await RunJob(message as ExecuteJobRequest);
            }
            else
            {
                //Unhandled.
                return false;
            }
            return true;
        }
        public async Task RunJob(ExecuteJobRequest request)
        {
            try
            {
                var res = await _jobExecutor.ExecuteJobAsync(request.JobData);
                Context.Sender.Tell(new JobSuceeded(request.JobData,res));
            }
            catch(Exception ex)
            {
                Context.Sender.Tell(new JobFailed(request.JobData, ex));
            }
        }
    }
}
