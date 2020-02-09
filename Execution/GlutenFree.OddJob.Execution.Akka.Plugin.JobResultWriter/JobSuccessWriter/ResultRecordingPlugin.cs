using System;
using Akka.Actor;
using Akka.Routing;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class ResultRecordingPlugin : ActorBase
    {
        public static IJobExecutionPluginConfiguration PropFactory(
            Func<IJobQueueResultWriter> writer, int numRecorders)
        {
            return new JobExecutionResultRecordingPluginConfiguration(
                Props.Create(() => new ResultRecordingPlugin()),
                new SetRecordingConfiguration()
                {
                    NumRecorders = numRecorders,
                    RecorderProps =
                        Props.Create(() => new JobSuccessWriter(writer()))
                });
        }

        private int NumWorkers = 0;
        private int ShutdownCount = 0;
        private IActorRef recorder;
        protected override bool Receive(object message)
        {
            if (message is SetRecordingConfiguration _r)
            {
                recorder = Context.ActorOf(
                    _r.RecorderProps.WithRouter(
                        new RoundRobinPool(_r.NumRecorders)), "recorder");
                NumWorkers = _r.NumRecorders;
            }
            else if (message is ShutDownQueues) 
            {
                recorder.Tell(new Broadcast(new ShutDownQueues()));
            }
            else if (message is QueueShutDown)
            {
                ShutdownCount = ShutdownCount + 1;
                if (NumWorkers <= ShutdownCount)
                {
                    Context.Parent.Tell(new QueueShutDown());
                }
            }
            else if (message is JobSuceeded)
            {
                recorder.Tell(message);
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}