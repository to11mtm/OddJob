using System;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    static class Mapping
    {
        public static MappingSchema BuildMappingSchema(ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration)
        {
            var mapper = new LinqToDB.Mapping.FluentMappingBuilder(MappingSchema.Default);
            mapper.Entity<SqlCommonDbOddJobMetaData>().HasAttribute(
                new TableAttribute(_jobQueueTableConfiguration.QueueTableName) { IsColumnAttributeRequired = false, Name = _jobQueueTableConfiguration.QueueTableName});
            mapper.Entity<SqlCommonOddJobParamMetaData>().HasAttribute(
                new TableAttribute(_jobQueueTableConfiguration.ParamTableName){IsColumnAttributeRequired = false, Name = _jobQueueTableConfiguration.ParamTableName});
            return mapper.MappingSchema;
        }
    }
    public abstract class BaseSqlJobQueueAdder : IJobQueueAdder
    {
        ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration;
        private readonly MappingSchema _mappingSchema;
        
        protected BaseSqlJobQueueAdder(IJobQueueDbConnectionFactory jobQueueDbConnectionFactory, ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration)
        {
            _jobQueueConnectionFactory = jobQueueDbConnectionFactory;
            //FormattedMarkNewSql = string.Format(@"update {0} set Status='New' where Id = @jobId", _jobQueueTableConfiguration.QueueTableName);
            _jobQueueTableConfiguration = jobQueueTableConfiguration;


            _mappingSchema = Mapping.BuildMappingSchema(jobQueueTableConfiguration);
            

        }

        
        
        private IJobQueueDbConnectionFactory _jobQueueConnectionFactory { get; set; }
        public virtual Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
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
                    .Value(q => q.RetryCount, 0)
                    .InsertWithInt64Identity();


                /*var insertedId = conn.ExecuteScalar<int>(FormattedMainInsertSql,
                    new
                    {
                        queueName = queueName,
                        typeExecutedOn = ser.TypeExecutedOn.AssemblyQualifiedName,
                        methodName = ser.MethodName,
                        doNotExecuteBefore = executionTime,
                        jobGuid = ser.Id,
                        maxRetries = (retryParameters == null ? 0 : (int?)retryParameters.MaxRetries),
                        minRetryWait = (retryParameters == null ? 0 : (double?)retryParameters.MinRetryWait.TotalSeconds),
                        retryCount = 0
                    }
                );*/
                var toInsert = ser.JobArgs.Select((val, index) => new { val, index }).ToList();
                toInsert.ForEach(i =>
                {

                    conn.GetTable<SqlCommonOddJobParamMetaData>()
                        .Value(q => q.Id, ser.JobId)
                        .Value(q => q.ParamOrdinal, i.index)
                        .Value(q => q.SerializedValue, Newtonsoft.Json.JsonConvert.SerializeObject(i.val))
                        .Value(q => q.SerializedType, i.val.GetType().AssemblyQualifiedName)
                        .Insert();
                    
                    /*   conn.ExecuteScalar<int>(FormattedParamInsertSql,
                           new
                           {
                               jobId = ser.Id,
                               paramOrdinal = i.index,
                               serializedValue =
                                   Newtonsoft.Json.JsonConvert.SerializeObject(i.val),
                               serializedType = i.val.GetType().AssemblyQualifiedName,
                           });*/
                });
                //conn.ExecuteScalar(FormattedMarkNewSql, new { jobId = insertedId });
                return ser.JobId;//*/
            }
        }
    }
}