using System;
using System.Linq;
using Dapper;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.Tools;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class BaseSqlJobQueuePurger : IJobQueuePurger
    {
        private readonly MappingSchema _mappingSchema;
        private readonly IJobQueueDbConnectionFactory _jobQueueConnectionFactory;
        private readonly ISqlDbJobQueueTableConfiguration _tableConfig;

        public string PurgeString { get; private set; }
        public BaseSqlJobQueuePurger(IJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            _tableConfig = tableConfig;
            PurgeString = string.Format(@"
delete from {0} where Id in (select JobGuid from {1} where QueueName = @queueName and Status=@status and CreatedDate<@jobOlderThan)
delete from {1} where QueueName = @queueName and Status = @status and CreatedDate<@jobOlderThan", _tableConfig.ParamTableName, _tableConfig.QueueTableName);

            _mappingSchema = Mapping.BuildMappingSchema(tableConfig);
        }

        public void PurgeQueue(string name, string stateToPurge, DateTime purgeOlderThan)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
            {
                conn.GetTable<SqlCommonOddJobParamMetaData>()
                    .Where(q => q.Id.In(conn.GetTable<SqlCommonDbOddJobMetaData>()
                        .Where(r => r.Status == stateToPurge && r.CreatedDate < purgeOlderThan && r.QueueName == name).Select(r=>r.JobGuid)))
                    .Delete();
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(r => r.Status == stateToPurge && r.CreatedDate < purgeOlderThan && r.QueueName == name)
                    .Delete();
                   
            }
        }
    }
}