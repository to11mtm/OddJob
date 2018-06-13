namespace OddJob.Execution.Akka
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
