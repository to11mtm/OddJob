using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{

    public class SqlServerJobQueueManager : BaseSqlJobQueueManager<SqlServerDBConnectionFactory>
    {
        
        public SqlServerJobQueueManager(SqlServerDBConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig) : base(jobQueueConnectionFactory,
            tableConfig)
        {

        }
        
    }
}
