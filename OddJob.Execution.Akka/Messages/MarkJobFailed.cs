using System;

namespace OddJob.Execution.Akka
{
    public class MarkJobFailed
    {
        public Guid JobId { get; protected set; }

        public MarkJobFailed(Guid jobId)
        {
            this.JobId = jobId;
        }
    }
}