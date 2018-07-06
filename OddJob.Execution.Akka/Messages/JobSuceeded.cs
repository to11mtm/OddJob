using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Messages
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
