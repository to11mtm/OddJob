using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public interface IJobExecutor
    {
        void ExecuteJob(IOddJob job);
    }
}