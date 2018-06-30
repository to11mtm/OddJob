using System;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using OddJob.Storage.SQL.Common;

namespace OddJob.Storage.SqlServer
{
    public abstract class BaseSqlJobQueueAdder : IJobQueueAdder
    {
        public abstract string FormattedMainInsertSql { get; protected set; }
        public abstract string FormattedParamInsertSql { get; protected set; }
        public abstract string FormattedMarkNewSql { get; protected set; }
        protected abstract IJobQueueDbConnectionFactory _jobQueueConnectionFactory { get; set; }
        public virtual Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                var ser = JobCreator.Create(jobExpression);
                var insertedId = conn.ExecuteScalar<int>(FormattedMainInsertSql,
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
                    conn.ExecuteScalar<int>(FormattedParamInsertSql,
                        new
                        {
                            jobId = ser.JobId,
                            paramOrdinal = i.index,
                            serializedValue =
                                Newtonsoft.Json.JsonConvert.SerializeObject(i.val),
                            serializedType = i.val.GetType().AssemblyQualifiedName,
                        });
                });
                conn.ExecuteScalar(FormattedMarkNewSql, new { jobId = insertedId });
                return ser.JobId;
            }
        }
    }
}