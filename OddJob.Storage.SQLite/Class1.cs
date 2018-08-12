using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SqlLiteJobQueueManager : BaseSqlJobQueueManager
    {


        public SqlLiteJobQueueManager(IJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlServerJobQueueTableConfiguration tableConfiguration) : base(
            jobQueueConnectionFactory, tableConfiguration)
        {


        }

    }
}
