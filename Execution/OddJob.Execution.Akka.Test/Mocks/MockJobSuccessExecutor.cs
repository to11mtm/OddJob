using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Test.Mocks
{
    public class MockJobSuccessExecutor : IJobExecutor
    {
        public IOddJobResult ExecuteJob(IOddJob job)
        {
            return new OddJobResult()
            {
                ReturnType =
                    MethodInfoHelper.GetMethodInfoForExpr(job).ReturnType,
                Result = null
            };
        }

        public async Task<IOddJobResult> ExecuteJobAsync(IOddJob requestJobData)
        {
            return new OddJobResult()
            {
                ReturnType =
                    MethodInfoHelper.GetMethodInfoForExpr(requestJobData).ReturnType,
                Result = null
            };
        }
    }
}
