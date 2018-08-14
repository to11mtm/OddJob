using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SQLiteJobQueueAdder : BaseSqlJobQueueAdder<SQLiteJobQueueDbConnectionFactory>
    {
      
        public SQLiteJobQueueAdder(SQLiteJobQueueDbConnectionFactory jobQueueDbConnectionFactory, SQL.Common.ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration) : base(jobQueueDbConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
