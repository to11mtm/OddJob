using System;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    
    public class JobSuccessWriter : ActorBase
    {
        private IJobQueueResultWriter _writer;
        public JobSuccessWriter(IJobQueueResultWriter writer)
        {
            _writer = writer;
        }
        protected override bool Receive(object message)
        {
            if (message is JobSuceeded succeeded)
            {
                try
                {
                    _writer.WriteJobQueueResult(succeeded.JobData.JobId, succeeded.Result);
                }
                catch (Exception e)
                {
                    Context.System.Log.Error(e,
                        $"Exception writing Success Event for Job {succeeded.JobData.JobId}, Result {succeeded.Result.Result.ToString()}");
                }
            }
            else if (message is ShutDownQueues)
            {
                Context.Sender.Tell(new QueueShutDown());
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}