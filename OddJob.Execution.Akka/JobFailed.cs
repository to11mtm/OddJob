using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OddJob.Execution.Akka
{
    public class JobFailed
    {
        public JobFailed(IOddJob jobData, Exception exception)
        {
            JobData = jobData;
        }
        public IOddJob JobData { get; protected set; }
        public Exception exception { get; protected set; }
    }
}
