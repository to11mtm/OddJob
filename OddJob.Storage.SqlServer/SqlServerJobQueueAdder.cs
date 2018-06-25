using System;
using System.Linq;
using System.Linq.Expressions;
using Dapper;

namespace OddJob.Storage.SqlServer
{
    public class SqlServerJobQueueAdder : IJobQueueAdder
    {
        private IJobQueueDbConnectionFactory _jobQueueConnectionFactory;
        ISqlServerJobQueueTableConfiguration _jobQueueTableConfiguration;
        public SqlServerJobQueueAdder(IJobQueueDbConnectionFactory jobQueueDbConnectionFactory,ISqlServerJobQueueTableConfiguration jobQueueTableConfiguration)
        {
            _jobQueueConnectionFactory = jobQueueDbConnectionFactory;
            _jobQueueTableConfiguration = jobQueueTableConfiguration;
            formattedMainInsertSql = string.Format(
                    @"insert into {0} (QueueName,TypeExecutedOn,MethodName,Status, DoNotExecuteBefore,JobGuid, MaxRetries, MinRetryWait, CreatedDate, RetryCount)
                      values (@queueName,@typeExecutedOn,@methodName,'Inserting',@doNotExecuteBefore, @jobGuid, @maxRetries, @minRetryWait, getdate(), @retryCount)
                      select scope_identity()", _jobQueueTableConfiguration.QueueTableName);
            formattedMarkNewSql= string.Format(@"update {0} set Status='New' where JobId = @jobId", _jobQueueTableConfiguration.QueueTableName);
            formattedParamInsertSql = string.Format(
                    @"insert into {0} (JobId, ParamOrdinal,SerializedValue, SerializedType)
                      values (@jobId,@paramOrdinal, @serializedValue, @serializedType)", _jobQueueTableConfiguration.ParamTableName);
        }
        private string formattedMainInsertSql
        {
            get; set;
        }
        private string formattedMarkNewSql
        {
            get;set;
        }

        private string formattedParamInsertSql
        {
            get;set;
            
        }

        public Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                var ser = JobCreator.Create(jobExpression);
                var insertedId = conn.ExecuteScalar<int>(formattedMainInsertSql,
                    new
                    {
                        queueName = queueName,
                        typeExecutedOn = ser.TypeExecutedOn.AssemblyQualifiedName,
                        methodName = ser.MethodName,
                        doNotExecuteBefore = executionTime,
                        jobGuid = ser.JobId,
                        maxRetries = (retryParameters == null ? 0 : (int?)retryParameters.MaxRetries),
                        minRetryWait = (retryParameters == null ? 0 : (double?)retryParameters.MinRetryWait.TotalSeconds),
                        retryCount = 0
                    }
                    );
                var toInsert = ser.JobArgs.Select((val, index) => new { val, index }).ToList();
                toInsert.ForEach(i =>
                {
                    conn.ExecuteScalar<int>(formattedParamInsertSql,
                    new
                    {
                        jobId = ser.JobId,
                        paramOrdinal = i.index,
                        serializedValue =
                           Newtonsoft.Json.JsonConvert.SerializeObject(i.val),
                        serializedType = i.val.GetType().AssemblyQualifiedName,
                    });
                });
                conn.ExecuteScalar(formattedMarkNewSql, new { jobId = insertedId });
                return ser.JobId;
            }
        }
    }
}
