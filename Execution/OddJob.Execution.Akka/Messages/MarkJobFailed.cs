using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
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