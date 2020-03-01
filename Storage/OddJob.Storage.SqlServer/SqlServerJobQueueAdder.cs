using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerJobQueueAdder : BaseSqlJobQueueAdder
    {


        public SqlServerJobQueueAdder(SqlServerDataConnectionFactory jobQueueDataConnectionFactory,
            IJobAdderQueueTableResolver jobQueueTableConfiguration) : base(jobQueueDataConnectionFactory,
            jobQueueTableConfiguration)
        {
        }
    }
}
