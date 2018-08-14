using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SqlLiteJobQueueManager : BaseSqlJobQueueManager<SQLiteJobQueueDbConnectionFactory>
    {
        public SqlLiteJobQueueManager(SQLiteJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfiguration) : base(
            jobQueueConnectionFactory, tableConfiguration)
        {
        }
    }
}
