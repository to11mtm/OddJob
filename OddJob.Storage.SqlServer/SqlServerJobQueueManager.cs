using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{

    public class SqlServerJobQueueManager : BaseSqlJobQueueManager
    {
        
        public SqlServerJobQueueManager(SqlServerDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig) : base(jobQueueConnectionFactory,
            tableConfig)
        {

        }
        
    }
}
