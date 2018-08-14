using System;
using System.Linq;
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
        private readonly IJobQueueDataConnectionFactory _jobQueueConnectionFactory;

        public BaseSqlJobQueuePurger(IJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;

            _mappingSchema = Mapping.BuildMappingSchema(tableConfig);
        }

        public void PurgeQueue(string name, string stateToPurge, DateTime purgeOlderThan)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
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