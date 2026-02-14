using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{

    public class SqlServerJobQueueManager : BaseSqlJobQueueManager
    {
        public SqlServerJobQueueManager(SqlServerDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig, IJobTypeResolver typeResolver) : base(jobQueueConnectionFactory,
            tableConfig, typeResolver)
        { }
        
    }
}
