using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SQLiteJobQueueManager : BaseSqlJobQueueManager
    {
        public SQLiteJobQueueManager(SQLiteJobQueueDataConnectionFactory jobQueueDataConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfiguration, IStorageJobTypeResolver typeResolver) : base(
            jobQueueDataConnectionFactory, tableConfiguration, typeResolver)
        {

        }
        public SQLiteJobQueueManager(SQLiteJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfiguration) : base(
            jobQueueConnectionFactory, tableConfiguration)
        {
        }
    }
}
