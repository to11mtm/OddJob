using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class JobSuceeded
    {
        public JobSuceeded(IOddJob jobData, IOddJobResult result)
        {
            JobData = jobData;
            Result = result;
        }
        public IOddJob JobData { get; protected set; }
        public IOddJobResult Result { get; protected set; }
    }
}
