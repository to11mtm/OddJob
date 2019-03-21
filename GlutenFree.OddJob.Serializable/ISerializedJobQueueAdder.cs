using System.Collections.Generic;

namespace GlutenFree.OddJob.Serializable
{
    public interface ISerializedJobQueueAdder
    {
        void AddJob(SerializableOddJob jobData);
        void AddJobs(IEnumerable<SerializableOddJob> jobDataSet);
    }
}