using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public record GetSpecificJob(Guid JobId, string QueueName) : IMarkJobCommand
    {
    }
}