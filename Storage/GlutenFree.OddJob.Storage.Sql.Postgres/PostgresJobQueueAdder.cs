using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.Postgres
{
    public class PostgresJobQueueAdder : BaseSqlJobQueueAdder
    {
        public PostgresJobQueueAdder(PostgresJobQueueDataConnectionFactory jobQueueDataConnectionFactory, IJobAdderQueueTableResolver jobQueueTableConfiguration)
            : base(jobQueueDataConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}

