using System;

namespace OddJob.Execution.Akka
{
    public class MarkJobSuccess
    {
        public Guid JobId { get; protected set; }

        public MarkJobSuccess(Guid jobId)
        {
            this.JobId = jobId;
        }
    }
}