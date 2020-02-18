using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public interface IMarkJobCommand
    {
        Guid JobId { get; }
    }
    public class MarkJobFailed : IMarkJobCommand
    {
        public Guid JobId { get; protected set; }

        public MarkJobFailed(Guid jobId)
        {
            this.JobId = jobId;
        }
    }
}