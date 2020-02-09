using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class GetSpecificJob
    {
        public Guid JobId { get; }
        public string QueueName { get; }

        public GetSpecificJob(Guid jobId, string queueName)
        {
            JobId = jobId;
            QueueName = queueName;
        }
    }
}