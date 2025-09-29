using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public interface IMarkJobCommand
    {
        Guid JobId { get; }
    }
    public record MarkJobFailed(Guid JobId) : IMarkJobCommand
    {
    }
}