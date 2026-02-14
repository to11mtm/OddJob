using GlutenFree.OddJob.Serializable;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    /// <summary>
    /// This interface lets you configure Queue table configurations for specific jobs.
    /// This can be useful in distributed scenarios where you wish to have multiple jobs written to different queues
    /// But want to keep everything in a single operation.
    /// </summary>
    public interface IJobAdderQueueTableResolver
    {
        ISqlDbJobQueueTableConfiguration GetConfigurationForJob(SerializableOddJob job);
    }
}