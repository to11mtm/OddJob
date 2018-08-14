using System;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public abstract class BaseSqlJobQueueAdder : IJobQueueAdder
    {
        private readonly MappingSchema _mappingSchema;
        
        protected BaseSqlJobQueueAdder(IJobQueueDataConnectionFactory jobQueueDataConnectionFactory, ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration)
        {
            _jobQueueConnectionFactory = jobQueueDataConnectionFactory;

            _mappingSchema = Mapping.BuildMappingSchema(jobQueueTableConfiguration);
        }

        
        
        private IJobQueueDataConnectionFactory _jobQueueConnectionFactory { get; set; }
        public virtual Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                var ser = JobCreator.Create(jobExpression);
                
                var insertedId = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Value(q => q.QueueName, queueName)
                    .Value(q => q.TypeExecutedOn, ser.TypeExecutedOn.AssemblyQualifiedName)
                    .Value(q => q.MethodName, ser.MethodName)
                    .Value(q => q.DoNotExecuteBefore, executionTime)
                    .Value(q => q.JobGuid, ser.JobId)
                    .Value(q=>q.Status, JobStates.New)
                    .Value(q=>q.CreatedDate, DateTime.Now)
                    .Value(q => q.MaxRetries, (retryParameters == null ? 0 : (int?) retryParameters.MaxRetries))
                    .Value(q => q.MinRetryWait,
                        retryParameters == null ? 0 : (double?) retryParameters.MinRetryWait.TotalSeconds)
                    .Value(q => q.RetryCount, 0);
                    insertedId.InsertWithInt64Identity();

                
                var toInsert = ser.JobArgs.Select((val, index) => new { val, index }).ToList();
                toInsert.ForEach(i =>
                {

                    conn.GetTable<SqlCommonOddJobParamMetaData>()
                        .Value(q => q.Id, ser.JobId)
                        .Value(q => q.ParamOrdinal, i.index)
                        .Value(q => q.SerializedValue, Newtonsoft.Json.JsonConvert.SerializeObject(i.val))
                        .Value(q => q.SerializedType, i.val.GetType().AssemblyQualifiedName)
                        .Insert();
                    
                    
                });
                
                return ser.JobId;
            }
        }
    }
}