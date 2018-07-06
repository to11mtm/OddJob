using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class ExecuteJobRequest
    {
        public ExecuteJobRequest(IOddJobWithMetadata jobData)
        {
            JobData = jobData;
        }
        public IOddJobWithMetadata JobData { get; protected set; }
    }
}
