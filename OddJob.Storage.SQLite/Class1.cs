using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SqlLiteJobQueueManager : BaseSqlJobQueueManager
    {
        private readonly ISqlServerJobQueueTableConfiguration _tableConfig
            ;

        public SqlLiteJobQueueManager(IJobQueueDbConnectionFactory jobQueueConnectionFactory, ISqlServerJobQueueTableConfiguration tableConfiguration) : base(jobQueueConnectionFactory)
        {
            _tableConfig = tableConfiguration;

            GetJobParamSqlString = string.Format(@"
select JobId, ParamOrdinal,SerializedValue, SerializedType
from {0} where JobId in (@jobIds)", _tableConfig.ParamTableName);
            JobFailedString = string.Format("update {0} set status='{1}', LockClaimTime = null where JobGuid = @jobGuid",
                _tableConfig.QueueTableName, JobStates.Failed);
            JobSuccessString = string.Format("update {0} set status='{1}', LockClaimTime = null where JobGuid = @jobGuid",
                _tableConfig.QueueTableName, JobStates.Processed);
            JobInProgressString =
                string.Format("update {0} set status='{1}', LockClaimTime=null, LastAttempt=getDate() where JobGuid = @jobGuid",
                    _tableConfig.QueueTableName, JobStates.InProgress);
            JobRetryIncrementString =
                string.Format(
                    "update {0} set status='{1}', RetryCount = RetryCount + 1, LastAttempt=getDate(), LockClaimTime=null where JobGuid = @jobGuid",
                    _tableConfig.QueueTableName, JobStates.Retry);
            preFormattedLockSqlString = string.Format(lockStringToFormatBeforeTopNumber, _tableConfig.QueueTableName, _tableConfig.JobClaimLockTimeoutInSeconds) +
                                        "{0}" + string.Format(lockStringToFormatAfterTopNumber,
                                            _tableConfig.QueueTableName);
            JobByIdString =
                string.Format(
                    "select JobGuid, QueueName, TypeExecutedOn, MethodName, Status, DoNotExecuteBefore, MaxRetries, MinRetryWait, RetryCount from {0} where jobGuid = @jobGuid",
                    _tableConfig.QueueTableName);
        }
        private string preFormattedLockSqlString { get; set; }
        public override string GetJobParamSqlString { get; protected set; }
        public override string JobSuccessString { get; protected set; }
        public override string JobFailedString { get; protected set; }
        public override string GetJobSqlStringWithLock(int fetchSize)
        {
            return
                string.Format(preFormattedLockSqlString, fetchSize);
        }

        public override string JobInProgressString { get; protected set; }
        public override string JobRetryIncrementString { get; protected set; }
        public override string JobByIdString { get; protected set; }

        private const string lockStringToFormatBeforeTopNumber = @"
begin
update {0} set LockClaimTime=@claimTime, LockGuid = @lockGuid
where JobId in(
select a.JobId from (select JobId, CASE WHEN IFNULL(CreatedDate,datetime('1753-01-01')) > IFNULL(LastAttempt,datetime('1753-01-01')) THEN CreatedDate
            ELSE LastAttempt
       END AS MostRecentDate
        from {0} 
        where QueueName in (@queueNames) 
            and (DoNotExecuteBefore <=getdate() 
               or DoNotExecuteBefore is null)
            and (Status ='New' or (Status='Retry' and MaxRetries>=RetryCount and dateadd(second,MinRetryWait,LastAttempt)<=getdate()))
            and (LockClaimTime is null or LockClaimTime < dateadd(second,0-{1},lockClaimTime))
) a
order by a.MostRecentDate asc
limit ";
        private const string lockStringToFormatAfterTopNumber = @"
        )
select JobGuid, QueueName, TypeExecutedOn, MethodName, Status, DoNotExecuteBefore, MaxRetries, MinRetryWait, RetryCount
from {0}
where LockGuid = @lockGuid
end";
    }
}
