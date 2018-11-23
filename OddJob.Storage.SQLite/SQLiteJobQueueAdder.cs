using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SQLiteJobQueueAdder : BaseSqlJobQueueAdder
    {
      
        public SQLiteJobQueueAdder(SQLiteJobQueueDataConnectionFactory jobQueueDataConnectionFactory, IJobAdderQueueTableResolver jobQueueTableConfiguration) : base(jobQueueDataConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
