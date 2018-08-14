using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SQLiteJobQueueAdder : BaseSqlJobQueueAdder
    {
      
        public SQLiteJobQueueAdder(SQLiteJobQueueDataConnectionFactory jobQueueDataConnectionFactory, SQL.Common.ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration) : base(jobQueueDataConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
