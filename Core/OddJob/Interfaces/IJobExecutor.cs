using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public interface IJobExecutor
    {
        Task<IOddJobResult> ExecuteJobAsync(IOddJob requestJobData);
    }
}