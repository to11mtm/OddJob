using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Test.Mocks
{
    public class MockJobFailureExecutor : IJobExecutor
    {
        public IOddJobResult ExecuteJob(IOddJob job)
        {
            throw new Exception("Failed!");
        }
    }
}
