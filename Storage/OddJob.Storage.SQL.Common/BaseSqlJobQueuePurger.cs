using System;
using System.Linq;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.Tools;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    public class BaseSqlJobQueuePurger : IJobQueuePurger
    {
        private readonly FluentMappingBuilder _mappingSchema;
        private readonly IJobQueueDataConnectionFactory _jobQueueConnectionFactory;
        private ISqlDbJobQueueTableConfiguration _tableConfig;
        public BaseSqlJobQueuePurger(IJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            _tableConfig = tableConfig;
            _mappingSchema = Mapping.BuildMappingSchema(tableConfig);
        }

        public void PurgeQueue(string name, string stateToPurge, DateTime purgeOlderThan)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                conn.GetTable<SqlCommonOddJobParamMetaData>()
                    .TableName(_tableConfig.ParamTableName)
                    .Where(q => q.JobGuid.In(conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(_tableConfig.QueueTableName)
                        .Where(r => r.Status == stateToPurge && r.CreatedDate < purgeOlderThan && r.QueueName == name).Select(r=>r.JobGuid)))
                    .Delete();
                conn.GetTable<SqlDbOddJobMethodGenericInfo>()
                    .TableName(_tableConfig.JobMethodGenericParamTableName)
                    .Where(q => q.JobGuid.In(conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(_tableConfig.QueueTableName)
                        .Where(r => r.Status == stateToPurge && r.CreatedDate < purgeOlderThan && r.QueueName == name).Select(r => r.JobGuid)))
                    .Delete();
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .TableName(_tableConfig.QueueTableName)
                    .Where(r => r.Status == stateToPurge && r.CreatedDate < purgeOlderThan && r.QueueName == name)
                    .Delete();
                   
            }
        }
    }
}