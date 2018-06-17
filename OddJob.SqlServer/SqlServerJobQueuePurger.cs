using System;
using Dapper;

namespace OddJob.SqlServer
{
    public class SqlServerJobQueuePurger : IJobQueuePurger
    {
        private IJobQueueDbConnectionFactory _jobQueueConnectionFactory;
        private ISqlServerJobQueueTableConfiguration _tableConfig;

        public string PurgeString { get; private set; }
        public SqlServerJobQueuePurger(IJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlServerJobQueueTableConfiguration tableConfig)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            _tableConfig = tableConfig;
            PurgeString = string.Format(@"
delete from {0} where JobGuid in (select JobGuid from {1} where QueueName = @queueName and Status=@status and CreatedDate<@jobOlderThan)
delete from {1} where QueueName = @queueName and Status = @status and CreatedDate<@jobOlderThan", _tableConfig.ParamTableName, _tableConfig.QueueTableName);
        }

        public void PurgeQueue(string name, string stateToPurge, DateTime purgeOlderThan)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(PurgeString, new {queueName = name, status = stateToPurge, jobOlderThan = purgeOlderThan});
            }
        }
    }
}