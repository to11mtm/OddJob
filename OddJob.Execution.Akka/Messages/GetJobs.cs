using System;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class GetJobs
    {
        public string QueueName { get; protected set; }
        public int FetchSize { get; protected set; }
        public GetJobs(string queueName,int fetchSize, Guid sweepGuid)
        {
            QueueName = queueName;
            FetchSize = fetchSize;
            SweepGuid = sweepGuid;
        }

        public Guid SweepGuid { get; set; }
    }
}

