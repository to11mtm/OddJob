using System;

namespace OddJob.Execution.Akka
{
    public class MarkJobInProgress
    {
        public Guid JobId { get; protected set; }

        public MarkJobInProgress(Guid jobId)
        {
            this.JobId = jobId;
        }
    }
}