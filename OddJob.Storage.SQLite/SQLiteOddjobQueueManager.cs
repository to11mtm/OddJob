using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
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
