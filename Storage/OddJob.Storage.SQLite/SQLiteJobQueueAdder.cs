using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.SQLite
{
    public class SQLiteJobQueueAdder : BaseSqlJobQueueAdder
    {
      
        public SQLiteJobQueueAdder(SQLiteJobQueueDataConnectionFactory jobQueueDataConnectionFactory, IJobAdderQueueTableResolver jobQueueTableConfiguration) : base(jobQueueDataConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
