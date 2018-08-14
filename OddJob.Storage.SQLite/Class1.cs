using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SqlLiteJobQueueManager : BaseSqlJobQueueManager
    {


        public SqlLiteJobQueueManager(IJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfiguration) : base(
            jobQueueConnectionFactory, tableConfiguration)
        {


        }

    }
}
