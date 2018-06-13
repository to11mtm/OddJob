using System;

namespace OddJob.Execution.Akka
{
    public class MarkJobInRetryAndIncrement
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

