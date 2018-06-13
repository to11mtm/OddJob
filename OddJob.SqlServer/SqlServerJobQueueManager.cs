using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
namespace OddJob.SqlServer
{
    public class SqlServerJobQueueManager : IJobQueueManager
    {
        public int FetchSize { get; protected set; }
        public string QueueTableName { get { return "MainQueueTable"; } }
        public string ParamTableName { get { return "QueueParamValue"; } }
        private SqlConnection conn { get; set; }
        public IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames)
        {
            var lockGuid = Guid.NewGuid();
            var lockTime = DateTime.Now;
            var baseJobs = conn.Query<SqlServerDbOddJobMetaData>(GetJobSqlStringWithLock, new { queueNames = queueNames, lockGuid = lockGuid, claimTime = lockTime });

            var jobMetaData = conn.Query<SqlServerOddJobParamMetaData>(GetJobParamSqlString, new { jobIds = baseJobs.Select(q=>q.JobId).ToList() });
            return baseJobs
                .GroupJoin(jobMetaData,
                q => q.JobId,
                r => r.JobId,
                (q, r) =>
                new SqlServerDbOddJob()
                {
                    JobId = q.JobGuid,
                    MethodName = q.MethodName,
                    TypeExecutedOn = Type.GetType(q.TypeExecutedOn),
                    JobArgs = r.OrderBy(p => p.ParamOrdinal)
                    .Select(s =>
                    Newtonsoft.Json.JsonConvert.DeserializeObject(s.SerializedValue,Type.GetType(s.SerializedType,false))).ToArray(),
                    RetryParameters= new RetryParameters(q.MaxRetries, TimeSpan.FromSeconds(q.MinRetryWait), q.RetryCount,q.LastAttempt )
                });

        }

        public string GetJobParamSqlString { get
            {
                return string.Format(@"
select JobId, ParamOrdinal,SerializedValue, SerializedType
from {0} where JobId in (@jobIds)", ParamTableName);
            } }
        public string GetJobSqlStringWithLock {get{
                return
string.Format(@"
begin
update {1} set LockClaimTime=@claimTime, LockGuid = @lockGuid
where JobId in(
select top {0}
         JobGuid, QueueName,TypeExecutedOn,
         MethodName,Status, 
         DoNotExecuteBefore 
        from {1} 
        where QueueName in (@queueNames) 
            and (DoNotExecuteBefore <=get_date() 
               or DoNotExecuteBefore is null)
            and (Status ='New' or (Status='Retry' and MaxRetry>AttemptCount and dateadd(seconds,MinRetryWait,LastAttempt<=getdate())))
            and (LockClaimTime is null or LockClaimTime < dateadd(seconds,lockClaimTime,0-180))
        )
select JobGuid, QueueName, TypeExecutedOn, MethodName, Status, DoNotExecuteBefore, MaxRetries, MinRetryWait, RetryCount
from {1}
where LockGuid = @lockGuid
)
end
"
, FetchSize, QueueTableName);
} }

        public void MarkJobFailed(Guid jobGuid)
        {
            conn.Execute(string.Format("update {0} set status='Failed' where JobGuid = @jobGuid", QueueTableName), new { jobGuid = jobGuid });
        }

        public void MarkJobSuccess(Guid jobGuid)
        {
            conn.Execute(string.Format("update {0} set status='Processed' where JobGuid = @jobGuid",QueueTableName), new { jobGuid = jobGuid });
        }

        public void MarkJobInProgress(Guid jobId)
        {
            conn.Execute(string.Format("update {0} set status='In-Process', LastAttempt=getDate() where JobGuid = @jobGuid", QueueTableName), new { jobGuid = jobId });
        }
        public void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            conn.Execute(string.Format("update {0} set status='Retry', RetryCount = RetryCount + 1, LastAttempt=getDate() where JobGuid = @jobGuid", QueueTableName), new { jobGuid = jobId });
        }
    }
}
