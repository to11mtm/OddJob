using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.Postgres
{
    public class PostgresJobQueueManager : BaseSqlJobQueueManager
    {
        public PostgresJobQueueManager(PostgresJobQueueDataConnectionFactory jobQueueDataConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfiguration, IJobTypeResolver typeResolver)
            : base(jobQueueDataConnectionFactory, tableConfiguration, typeResolver)
        {
        }
    }
}

