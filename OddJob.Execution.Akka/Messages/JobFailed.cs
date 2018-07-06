using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class JobFailed
    {
        public JobFailed(IOddJobWithMetadata jobData, Exception exception)
        {
            JobData = jobData;
            Exception = exception;
        }
        public IOddJobWithMetadata JobData { get; protected set; }
        public Exception Exception { get; protected set; }
    }
}
