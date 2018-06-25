using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using OddJob.Storage.SqlServer.DbDtos;

namespace OddJob.Storage.SqlServer
{
    public class SqlServerJobQueueManager : IJobQueueManager
    {
        ISqlServerJobQueueTableConfiguration _tableConfig;
        private IJobQueueDbConnectionFactory _jobQueueConnectionFactory { get; set; }

        public string GetJobParamSqlString { get; private set; }
        public string JobFailedString { get; private set; }
        public string JobSuccessString { get; private set; }
        public string JobInProcessString { get; private set; }
        public string JobRetryIncrementString { get; private set; }
public string JobByIdString { get; private set; }

        public SqlServerJobQueueManager(IJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlServerJobQueueTableConfiguration tableConfig)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            _tableConfig = tableConfig;
            GetJobParamSqlString = string.Format(@"
select JobId, ParamOrdinal,SerializedValue, SerializedType
from {0} where JobId in (@jobIds)", _tableConfig.ParamTableName);
            JobFailedString = string.Format("update {0} set status='Failed', LockClaimTime = null where JobGuid = @jobGuid",
                _tableConfig.QueueTableName);
            JobSuccessString = string.Format("update {0} set status='Processed', LockClaimTime = null where JobGuid = @jobGuid",
                _tableConfig.QueueTableName);
            JobInProcessString =
                string.Format("update {0} set status='In-Process', LockClaimTime=null, LastAttempt=getDate() where JobGuid = @jobGuid",
                    _tableConfig.QueueTableName);
            JobRetryIncrementString =
                string.Format(
                    "update {0} set status='Retry', RetryCount = RetryCount + 1, LastAttempt=getDate(), LockClaimTime=null where JobGuid = @jobGuid",
                    _tableConfig.QueueTableName);
            preFormattedLockSqlString = string.Format(lockStringToFormatBeforeTopNumber, _tableConfig.QueueTableName) +
                                        "{0}" + string.Format(lockStringToFormatAfterTopNumber,
                                            _tableConfig.QueueTableName, _tableConfig.JobClaimLockTimeoutInSeconds);
            JobByIdString =
                string.Format(
                    "select JobGuid, QueueName, TypeExecutedOn, MethodName, Status, DoNotExecuteBefore, MaxRetries, MinRetryWait, RetryCount from {0} where jobGuid = @jobGuid",
                    _tableConfig.QueueTableName);
        }

        private string preFormattedLockSqlString { get; set; }
        private Dictionary<int, string> fetchSqlCache = new Dictionary<int, string>();
        public IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize)
        {
            string myFetchString = null;
            if (fetchSqlCache.ContainsKey(fetchSize) == false)
            {
                fetchSqlCache[fetchSize] = GetJobSqlStringWithLock(fetchSize);
            }
            myFetchString = fetchSqlCache[fetchSize];
            var lockGuid = Guid.NewGuid();
            var lockTime = DateTime.Now;
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                var baseJobs = conn.Query<SqlServerDbOddJobMetaData>(myFetchString, new { queueNames = queueNames, lockGuid = lockGuid, claimTime = lockTime });

                var jobMetaData = conn.Query<SqlServerOddJobParamMetaData>(GetJobParamSqlString, new { jobIds = baseJobs.Select(q => q.JobGuid).ToList() });
                return baseJobs
                    .GroupJoin(jobMetaData,
                    q => q.JobGuid,
                    r => r.JobId,
                    (q, r) =>
                    new SqlServerDbOddJob()
                    {
                        JobId = q.JobGuid,
                        MethodName = q.MethodName,
                        TypeExecutedOn = Type.GetType(q.TypeExecutedOn),
                        JobArgs = r.OrderBy(p => p.ParamOrdinal)
                        .Select(s =>
                        Newtonsoft.Json.JsonConvert.DeserializeObject(s.SerializedValue, Type.GetType(s.SerializedType, false))).ToArray(),
                        RetryParameters = new RetryParameters(q.MaxRetries, TimeSpan.FromSeconds(q.MinRetryWait), q.RetryCount, q.LastAttempt)
                    });
            }
        }
        private const string lockStringToFormatBeforeTopNumber = @"
begin
update {0} set LockClaimTime=@claimTime, LockGuid = @lockGuid
where JobId in(
select top ";
        private const string lockStringToFormatAfterTopNumber = @"
a.JobId from (select JobId, CASE WHEN ISNULL(CreatedDate,'01-01-1753') > ISNULL(LastAttempt,'01-01-1753') THEN CreatedDate
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
        )
select JobGuid, QueueName, TypeExecutedOn, MethodName, Status, DoNotExecuteBefore, MaxRetries, MinRetryWait, RetryCount
from {0}
where LockGuid = @lockGuid
end";
        public string GetJobSqlStringWithLock(int fetchSize)
        {
            {
                return
string.Format(preFormattedLockSqlString,fetchSize);
            }
        }

        public void MarkJobFailed(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobFailedString, new { jobGuid = jobGuid });
            }
        } 
        
        public void MarkJobSuccess(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobSuccessString, new { jobGuid = jobGuid });
            }
        }

        public void MarkJobInProgress(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobInProcessString, new { jobGuid = jobId });
            }
        }
        public void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobRetryIncrementString, new { jobGuid = jobId });
            }
        }

        public IOddJobWithMetadata GetJob(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                var jobs = conn.Query<SqlServerDbOddJobMetaData>(JobByIdString, new {jobGuid = jobId});
                var jobMetaData = conn.Query<SqlServerOddJobParamMetaData>(GetJobParamSqlString,
                    new {jobIds = jobs.Select(q => q.JobGuid).ToList()});
                return jobs
                    .GroupJoin(jobMetaData,
                        q => q.JobGuid,
                        r => r.JobId,
                        (q, r) =>
                            new SqlServerDbOddJob()
                            {
                                JobId = q.JobGuid,
                                MethodName = q.MethodName,
                                TypeExecutedOn = Type.GetType(q.TypeExecutedOn),
                                JobArgs = r.OrderBy(p => p.ParamOrdinal)
                                    .Select(s =>
                                        Newtonsoft.Json.JsonConvert.DeserializeObject(s.SerializedValue, Type.GetType(s.SerializedType, false))).ToArray(),
                                RetryParameters = new RetryParameters(q.MaxRetries, TimeSpan.FromSeconds(q.MinRetryWait), q.RetryCount, q.LastAttempt),
                                Status = q.Status
                            }).FirstOrDefault();
            }
        }
    }
}
