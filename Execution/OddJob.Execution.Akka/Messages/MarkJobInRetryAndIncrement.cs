using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class MarkJobInRetryAndIncrement : IMarkJobCommand
    {
        public Guid JobId { get; protected set; }
        public DateTime LastAttempt { get; protected set; }

        public MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            this.JobId = jobId;
            this.LastAttempt = lastAttempt;
        }
    }
}

