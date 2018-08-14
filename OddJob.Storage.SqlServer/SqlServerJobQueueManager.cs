using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{

    public class SqlServerJobQueueManager : BaseSqlJobQueueManager<SqlServerDbConnectionFactory>
    {
        
        public SqlServerJobQueueManager(SqlServerDbConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig) : base(jobQueueConnectionFactory,
            tableConfig)
        {

        }
        
    }
}
