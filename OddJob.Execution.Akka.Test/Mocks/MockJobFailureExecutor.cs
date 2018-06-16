using System;

namespace OddJob.Execution.Akka.Test
{
    public class MockJobFailureExecutor : IJobExecutor
    {
        public void ExecuteJob(IOddJob job)
        {
            throw new Exception("Failed!");
        }
    }
}
