using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
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