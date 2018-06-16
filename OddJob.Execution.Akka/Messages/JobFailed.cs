using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OddJob.Execution.Akka
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
