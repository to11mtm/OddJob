using System;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public abstract class BaseSqlJobQueueAdder : IJobQueueAdder, ISerializedJobQueueAdder
    {
        private readonly MappingSchema _mappingSchema;
        
        protected BaseSqlJobQueueAdder(IJobQueueDataConnectionFactory jobQueueDataConnectionFactory, ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration)
        {
            _jobQueueConnectionFactory = jobQueueDataConnectionFactory;

            _mappingSchema = Mapping.BuildMappingSchema(jobQueueTableConfiguration);
        }

        
        
        private IJobQueueDataConnectionFactory _jobQueueConnectionFactory { get; set; }

        public virtual void AddJob(SerializableOddJob jobData)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                

                var insertedIdExpr = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Value(q => q.QueueName, jobData.QueueName)
                    .Value(q => q.TypeExecutedOn,jobData.TypeExecutedOn)
                    .Value(q => q.MethodName, jobData.MethodName)
                    .Value(q => q.DoNotExecuteBefore, jobData.ExecutionTime)
                    .Value(q => q.JobGuid, jobData.JobId)
                    .Value(q => q.Status, JobStates.Inserting)
                    .Value(q => q.CreatedDate, DateTime.Now)
                    .Value(q => q.MaxRetries, (jobData.RetryParameters == null ? 0 : (int?)jobData.RetryParameters.MaxRetries))
                    .Value(q => q.MinRetryWait,
                        jobData.RetryParameters == null ? 0 : (double?)jobData.RetryParameters.MinRetryWait.TotalSeconds)
                    .Value(q => q.RetryCount, 0);
                var insertedId = insertedIdExpr.InsertWithInt64Identity();


                var toInsert = jobData.JobArgs.Select((val, index) => new { val, index }).ToList();
                toInsert.ForEach(i =>
                {

                    conn.GetTable<SqlCommonOddJobParamMetaData>()
                        .Value(q => q.JobGuid, jobData.JobId)
                        .Value(q => q.ParamOrdinal, i.index)
                        .Value(q => q.SerializedValue, i.val.Value)
                        .Value(q => q.SerializedType, i.val.TypeName)
                        .Value(q => q.ParameterName, i.val.Name)
                        .Insert();


                });

                var genMethodArgs = jobData.MethodGenericTypes.Select((val, index) => new { val, index }).ToList();
                genMethodArgs.ForEach(i =>
                {
                    conn.GetTable<SqlDbOddJobMethodGenericInfo>()
                        .Value(q => q.JobGuid, jobData.JobId)
                        .Value(q => q.ParamOrder, i.index)
                        .Value(q => q.ParamTypeName, i.val)
                        .Insert();
                });

                conn.GetTable<SqlCommonDbOddJobMetaData>().Where(q => q.Id == insertedId)
                    .Set(q => q.Status, JobStates.New)
                    .Update();
            }

        }
        public virtual Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                var ser = JobCreator.Create(jobExpression);
                
                var insertedIdExpr = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Value(q => q.QueueName, queueName)
                    .Value(q => q.TypeExecutedOn, ser.TypeExecutedOn.AssemblyQualifiedName)
                    .Value(q => q.MethodName, ser.MethodName)
                    .Value(q => q.DoNotExecuteBefore, executionTime)
                    .Value(q => q.JobGuid, ser.JobId)
                    .Value(q=>q.Status, JobStates.Inserting)
                    .Value(q=>q.CreatedDate, DateTime.Now)
                    .Value(q => q.MaxRetries, (retryParameters == null ? 0 : (int?) retryParameters.MaxRetries))
                    .Value(q => q.MinRetryWait,
                        retryParameters == null ? 0 : (double?) retryParameters.MinRetryWait.TotalSeconds)
                    .Value(q => q.RetryCount, 0);
                    var insertedId = insertedIdExpr.InsertWithInt64Identity();

                
                var toInsert = ser.JobArgs.Select((val, index) => new { val, index }).ToList();
                toInsert.ForEach(i =>
                {

                    conn.GetTable<SqlCommonOddJobParamMetaData>()
                        .Value(q => q.JobGuid, ser.JobId)
                        .Value(q => q.ParamOrdinal, i.index)
                        .Value(q => q.SerializedValue, Newtonsoft.Json.JsonConvert.SerializeObject(i.val.Value))
                        .Value(q => q.SerializedType, i.val.Value.GetType().AssemblyQualifiedName)
                        .Value(q=>q.ParameterName, i.val.Name)
                        .Insert();
                    
                    
                });

                var genMethodArgs = ser.MethodGenericTypes.Select((val, index) => new {val, index}).ToList();
                genMethodArgs.ForEach(i =>
                {
                    conn.GetTable<SqlDbOddJobMethodGenericInfo>()
                        .Value(q => q.JobGuid, ser.JobId)
                        .Value(q => q.ParamOrder, i.index)
                        .Value(q => q.ParamTypeName, i.val.AssemblyQualifiedName)
                        .Insert();
                });

                conn.GetTable<SqlCommonDbOddJobMetaData>().Where(q => q.Id == insertedId)
                    .Set(q => q.Status, JobStates.New)
                    .Update();
                
                return ser.JobId;
            }
        }
    }
}