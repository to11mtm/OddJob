namespace OddJob.Execution.Akka
{
    public class ExecuteJobRequest
    {
        public ExecuteJobRequest(IOddJob jobData)
        {
            JobData = jobData;
        }
        public IOddJob JobData { get; protected set; }
    }
}
