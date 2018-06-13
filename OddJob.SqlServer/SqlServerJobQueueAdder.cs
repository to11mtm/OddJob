using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace OddJob.SqlServer
{
    public class SqlServerJobQueueAdder : IJobQueueAdder
    {
        private SqlConnection conn;

        public string MainQueueTableName { get { return "MainQueueTable"; } }
        public string ParamValueTable { get { return "QueueParamValue"; } }
        private string formattedMainInsertSql
        {
            get
            {
                return string.Format(
                    @"insert into {0} (QueueName,TypeExecutedOn,MethodName,Status, DoNotExecuteBefore,JobGuid, MaxRetries, MinRetryWait)
                      values (@queueName,@typeExecutedOn,@methodName,'New',@doNotExecuteBefore, @jobGuid, @maxRetries, @minRetryWait)
                      select scope_identity()",MainQueueTableName);
            }
        }

        private string formattedParamInsertSql
        {
            get
            {
                return string.Format(
                    @"insert into {0} (JobId, ParamOrdinal,SerializedValue, SerializedType)
                      values (@jobId,@paramOrdinal, @serializedValue, @serializedType)", ParamValueTable);
            }
        }

        public Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            var myGuid = Guid.NewGuid();
            var ser = JobCreator.Create(jobExpression);
            var insertedId = conn.ExecuteScalar<int>(formattedMainInsertSql,
                new
                {
                    queueName = queueName,
                    typeExecutedOn = ser.TypeExecutedOn.AssemblyQualifiedName,
                    methodName = ser.MethodName,
                    doNotExecuteBefore = executionTime,
                    jobGuid = ser.JobId,
                    maxRetries = (retryParameters == null ? null : (int?)retryParameters.MaxRetries),
                    minRetryWait = (retryParameters == null ? null : (double?)retryParameters.MinRetryWait.TotalSeconds)
                }
                );
            var toInsert = ser.JobArgs.Select((val, index) => new { val, index }).ToList();
            toInsert.ForEach(i => {
                conn.ExecuteScalar<int>(formattedParamInsertSql,
                new
                {
                    jobId = insertedId,
                    paramOrdinal = i.index,
                    serializedValue =
                       Newtonsoft.Json.JsonConvert.SerializeObject(i.val),
                    serializedType = i.val.GetType().AssemblyQualifiedName,
                });
            });
            return myGuid;
        }
    }
}
