using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public record MarkJobInProgress(Guid JobId) : IMarkJobCommand
    {
    }
}