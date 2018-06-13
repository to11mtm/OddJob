namespace OddJob.Execution.Akka
{
    public class JobSuceeded
    {
        public JobSuceeded(IOddJob jobData)
        {
            JobData = jobData;
        }
        public IOddJob JobData { get; protected set; }
    }
}
