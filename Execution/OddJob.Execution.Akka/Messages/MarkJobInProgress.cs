using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class MarkJobInProgress : IMarkJobCommand
    {
        public Guid JobId { get; protected set; }

        public MarkJobInProgress(Guid jobId)
        {
            this.JobId = jobId;
        }
    }
}