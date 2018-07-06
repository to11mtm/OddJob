using System;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class MockJobWorker : ActorBase
    {
        public IActorRef ForwardTo { get; set; }
        public MockJobWorker(IActorRef forwardTo)
        {
            ForwardTo = forwardTo;
        }
        protected override bool Receive(object message)
        {
            if (message is ExecuteJobRequest)
            {
                ForwardTo.Forward(message);
                if (message is ExecuteJobRequest)
                {
                    var msg = message as ExecuteJobRequest;
                    if (msg.JobData.MethodName=="Fail")
                    {
                        Context.Sender.Tell(new JobFailed(msg.JobData, new Exception("derp")));
                    }
                    else if (msg.JobData.MethodName =="Success")
                    {
                        Context.Sender.Tell(new JobSuceeded(msg.JobData));
                    }
                }
            }
            return true;
        }
    }
}
