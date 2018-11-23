using System.Collections.Generic;
using GlutenFree.OddJob.Serializable;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    /// <summary>
    /// Retrieves Table configurations based on the Queue Name for a job
    /// If no mapping is found, the default table is used.
    /// </summary>
    public class QueueNameBasedJobAdderQueueTableResolver : IJobAdderQueueTableResolver
    {
        private readonly Dictionary<string, ISqlDbJobQueueTableConfiguration> _queueTableMappings;
        private readonly ISqlDbJobQueueTableConfiguration _defaultTableConfiguration;

        public QueueNameBasedJobAdderQueueTableResolver(
            Dictionary<string, ISqlDbJobQueueTableConfiguration> queueTableConfigurations,
            ISqlDbJobQueueTableConfiguration defaultTableConfiguration)
        {
            _queueTableMappings = queueTableConfigurations;
            _defaultTableConfiguration = defaultTableConfiguration;
        }

        public ISqlDbJobQueueTableConfiguration GetConfigurationForJob(SerializableOddJob job)
        {
            if (_queueTableMappings.ContainsKey(job.QueueName))
            {
                return _queueTableMappings[job.QueueName];
            }

            return _defaultTableConfiguration;
        }
    }
}