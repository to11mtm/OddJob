using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using OddJob.Storage.SQL.Common.DbDtos;

namespace OddJob.Storage.SQL.Common
{
    public abstract class BaseSqlJobQueueManager : IJobQueueManager
    {
        public BaseSqlJobQueueManager(IJobQueueDbConnectionFactory jobQueueConnectionFactory)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
        }
        protected IJobQueueDbConnectionFactory _jobQueueConnectionFactory { get; private set; }
        private Dictionary<int, string> fetchSqlCache = new Dictionary<int, string>();
        public abstract string GetJobParamSqlString { get; protected set; }

        public virtual void MarkJobSuccess(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobSuccessString, new { jobGuid = jobGuid });
            }
        }

        public abstract string JobSuccessString { get; protected set; }

        public virtual void MarkJobFailed(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobFailedString, new { jobGuid = jobGuid });
            }
        }

        public abstract string JobFailedString { get; protected set; }

        public abstract string GetJobSqlStringWithLock(int fetchSize);

        public virtual IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize)
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
                var baseJobs = conn.Query<SqlCommonDbOddJobMetaData>(myFetchString, new { queueNames = queueNames, lockGuid = lockGuid, claimTime = lockTime });

                var jobMetaData = conn.Query<SqlCommonOddJobParamMetaData>(GetJobParamSqlString, new { jobIds = baseJobs.Select(q => q.JobGuid).ToList() });
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

        public virtual void MarkJobInProgress(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobInProgressString, new { jobGuid = jobId });
            }
        }

        public abstract string JobInProgressString { get; protected set; }

        public virtual void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                conn.Execute(JobRetryIncrementString, new { jobGuid = jobId });
            }
        }

        public abstract string JobRetryIncrementString { get; protected set; }

        public virtual IOddJobWithMetadata GetJob(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.GetConnection())
            {
                var jobs = conn.Query<SqlCommonDbOddJobMetaData>(JobByIdString, new { jobGuid = jobId });
                var jobMetaData = conn.Query<SqlCommonOddJobParamMetaData>(GetJobParamSqlString,
                    new { jobIds = jobs.Select(q => q.JobGuid).ToList() });
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

        public abstract string JobByIdString { get; protected set; }
    }
}
