using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    
    public class JobSuccessWriter : ReceiveActor
    {
        private IJobQueueResultWriter _writer;
        public JobSuccessWriter(IJobQueueResultWriter writer)
        {
            _writer = writer;
            ReceiveAsync<JobSuceeded>(HandleJobSuccess);
            Receive<ShutDownQueues>(HandleShutDown);
        }
        private void HandleShutDown(ShutDownQueues sd)
        {
            Context.Sender.Tell(new QueueShutDown());
        }
        private async Task HandleJobSuccess(JobSuceeded js)
        {
            try
            {
                await _writer.WriteJobQueueResult(js.JobData.JobId, js.Result);
            }
            catch (Exception e)
            {
                Context.System.Log.Error(e,
                    $"Exception writing Success Event for Job {js.JobData.JobId}, Result {js.Result.Result.ToString()}");
            }
        }
    }
}