using OddJob.Storage.SQL.Common;

namespace OddJob.Storage.SqlServer
{
    public class SqlServerJobQueueAdder : BaseSqlJobQueueAdder 
    {
        
        ISqlServerJobQueueTableConfiguration _jobQueueTableConfiguration;
        public SqlServerJobQueueAdder(IJobQueueDbConnectionFactory jobQueueDbConnectionFactory,ISqlServerJobQueueTableConfiguration jobQueueTableConfiguration)
        {
            _jobQueueConnectionFactory = jobQueueDbConnectionFactory;
            _jobQueueTableConfiguration = jobQueueTableConfiguration;
            FormattedMainInsertSql = string.Format(
                    @"insert into {0} (QueueName,TypeExecutedOn,MethodName,Status, DoNotExecuteBefore,JobGuid, MaxRetries, MinRetryWait, CreatedDate, RetryCount)
                      values (@queueName,@typeExecutedOn,@methodName,'Inserting',@doNotExecuteBefore, @jobGuid, @maxRetries, @minRetryWait, getdate(), @retryCount)
                      select scope_identity()", _jobQueueTableConfiguration.QueueTableName);
            FormattedMarkNewSql= string.Format(@"update {0} set Status='New' where JobId = @jobId", _jobQueueTableConfiguration.QueueTableName);
            FormattedParamInsertSql = string.Format(
                    @"insert into {0} (JobId, ParamOrdinal,SerializedValue, SerializedType)
                      values (@jobId,@paramOrdinal, @serializedValue, @serializedType)", _jobQueueTableConfiguration.ParamTableName);
        }


        public override string FormattedMainInsertSql { get; protected set; }
        public override string FormattedParamInsertSql { get; protected set; }
        public override string FormattedMarkNewSql { get; protected set; }
        protected override IJobQueueDbConnectionFactory _jobQueueConnectionFactory { get; set; }
    }
}
