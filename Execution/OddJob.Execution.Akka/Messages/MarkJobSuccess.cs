using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public record MarkJobSuccess(Guid JobId) : IMarkJobCommand
    {
    }
}