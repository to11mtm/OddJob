using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlutenFree.OddJob.Serializable
{
    public interface ISerializedJobQueueAdder
    {
        void AddJob(SerializableOddJob jobData);
        void AddJobs(IEnumerable<SerializableOddJob> jobDataSet);
    }
}