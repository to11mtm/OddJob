using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.SQLite
{
    public class SQLiteJobQueueManager : BaseSqlJobQueueManager
    {
        public SQLiteJobQueueManager(SQLiteJobQueueDataConnectionFactory jobQueueDataConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfiguration, IJobTypeResolver typeResolver) : base(
            jobQueueDataConnectionFactory, tableConfiguration, typeResolver)
        {

        }
    }
}
