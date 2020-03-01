using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GlutenFree.OddJob.Serializable
{
    public interface ISerializedJobQueueAdder
    {
        void AddJob(SerializableOddJob jobData);

        Task AddJobAsync(SerializableOddJob jobData,
            CancellationToken cancellationToken = default);
        void AddJobs(IEnumerable<SerializableOddJob> jobDataSet);

        Task AddJobsAsync(IEnumerable<SerializableOddJob> jobDataSet,
            CancellationToken cancellationToken = default);
    }
}