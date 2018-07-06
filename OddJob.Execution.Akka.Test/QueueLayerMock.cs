using System;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class QueueLayerMock : ActorBase
    {
        IActorRef ForwardTo;
        string GenerateJobType;
        public QueueLayerMock(IActorRef forwardTo, string generateJobType)
        {
            ForwardTo = forwardTo;
            GenerateJobType = generateJobType;
        }
        protected override bool Receive(object message)
        {
            ForwardTo.Forward(message);
            if (message is GetJobs)
            {
                Context.Sender.Tell(new[] { new OddJobWithMetaData() { CreatedOn = DateTime.Now, FailureTime = null, JobArgs = new object[] { 0, 1, "derp" }, JobId = Guid.NewGuid(), LastAttemptTime = null, MethodName = GenerateJobType, QueueTime = DateTime.Now, RetryParameters = null, Status = JobStates.New, TypeExecutedOn = "derp".GetType() } });
            }
            else return false;
            return true;
        }
    }
}
