using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public interface IJobExecutor
    {
        IOddJobResult ExecuteJob(IOddJob job);
        Task<IOddJobResult> ExecuteJobAsync(IOddJob requestJobData);
    }
}