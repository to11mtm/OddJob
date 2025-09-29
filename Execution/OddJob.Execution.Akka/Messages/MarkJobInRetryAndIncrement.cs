using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public record MarkJobInRetryAndIncrement(Guid JobId, DateTime LastAttempt) : IMarkJobCommand
    {
        public Guid JobId { get; protected set; } = JobId;
        public DateTime LastAttempt { get; protected set; } = LastAttempt;
    }
}

