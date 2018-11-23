using System.Collections.Generic;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public class DefaultJobAdderQueueTableResolver : QueueNameBasedJobAdderQueueTableResolver
    {

        public DefaultJobAdderQueueTableResolver(ISqlDbJobQueueTableConfiguration tableConfiguration) : base(
            new Dictionary<string, ISqlDbJobQueueTableConfiguration>(), tableConfiguration)
        {

        }

    }
}