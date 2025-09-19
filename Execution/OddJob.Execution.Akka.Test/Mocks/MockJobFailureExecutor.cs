using System;
using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Test.Mocks
{
    public class MockJobFailureExecutor : IJobExecutor
    {
        public IOddJobResult ExecuteJob(IOddJob job)
        {
            throw new Exception("Failed!");
        }

        public async Task<IOddJobResult> ExecuteJobAsync(IOddJob requestJobData)
        {
            throw new Exception("Failed!");
        }
    }
}
