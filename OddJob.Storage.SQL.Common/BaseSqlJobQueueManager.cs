using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Dapper;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{

    public class JobLockData
    {
        public long JobId { get; set; }
        public DateTime? MostRecentDate { get; set; }
    }
    public abstract class BaseSqlJobQueueManager : IJobQueueManager
    {
        ISqlServerJobQueueTableConfiguration _jobQueueTableConfiguration;
        private MappingSchema _mappingSchema = null;

        public BaseSqlJobQueueManager(IJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlServerJobQueueTableConfiguration jobQueueTableConfiguration)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            //FormattedMarkNewSql = string.Format(@"update {0} set Status='New' where Id = @jobId", _jobQueueTableConfiguration.QueueTableName);
            _jobQueueTableConfiguration = jobQueueTableConfiguration;


            _mappingSchema = Mapping.BuildMappingSchema(jobQueueTableConfiguration);
        }

        protected IJobQueueDbConnectionFactory _jobQueueConnectionFactory { get; private set; }
        

        public virtual void MarkJobSuccess(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
            {
                /*JobSuccessString = string.Format("update {0} set status='{1}', LockClaimTime = null where JobGuid = @jobGuid",
                _tableConfig.QueueTableName,JobStates.Processed);*/
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobGuid)
                    .Set(q => q.Status, JobStates.Processed)
                    .Set(q => q.LockClaimTime, (DateTime?) null)
                    .Update();
            }
        }


        public virtual void MarkJobFailed(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
            {
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobGuid)
                    .Set(q => q.Status, JobStates.Failed)
                    .Set(q => q.LockClaimTime,(DateTime?)null)
                    .Update();

            }
        }

        
        
        public virtual IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize)
        {
            var lockGuid = Guid.NewGuid();
            var lockTime = DateTime.Now;
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
            {

                IQueryable<JobLockData> lockingCheckQuery =
                    conn.GetTable<SqlCommonDbOddJobMetaData>()
                        .Where(
                            q => queueNames.Contains(q.QueueName)
                                 &&
                                 (q.DoNotExecuteBefore <= DateTime.Now || q.DoNotExecuteBefore == null)
                                 &&
                                 (q.Status == "New" ||
                                  (q.Status == "Retry" && q.MaxRetries >= q.RetryCount &&
                                   q.LastAttempt.Value.AddSeconds(q.MinRetryWait) <=DateTime.Now)

                                 )
                                 && (q.LockClaimTime == null || q.LockClaimTime <
                                     DateTime.Now.AddSeconds(
                                         (0) - _jobQueueTableConfiguration.JobClaimLockTimeoutInSeconds))
                        ).Select(q => new JobLockData
                        {
                            JobId = q.Id,
                            MostRecentDate =
                                (q.CreatedDate ?? DateTime.MinValue)
                                > (q.LastAttempt ?? DateTime.MinValue)
                                    ? q.CreatedDate
                                    : q.LastAttempt
                        }).OrderBy(q => q.MostRecentDate).Take(fetchSize);
                var updateQuery = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => lockingCheckQuery.Any(r => r.JobId == q.Id))
                    .Set(q => q.LockGuid, lockGuid)
                    .Set(q => q.LockClaimTime, lockTime)
                    .Update();
                        
                
                        //.Set(q => q.LockClaimTime, DateTime.Now)
                        //.Set(q => q.LockGuid, Guid.NewGuid());
                var jobWithParamQuery = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.LockGuid == lockGuid)
                    .InnerJoin(conn.GetTable<SqlCommonOddJobParamMetaData>()
                        , (job, param) => job.JobGuid == param.Id
                        , (job, param) => new {job, param});
                
                return jobWithParamQuery.ToList().GroupBy(q=>q.job.JobGuid)
                    .Select(group =>
                        new SqlServerDbOddJob()
                        {
                            JobId = group.Key,
                            MethodName = group.First().job.MethodName,
                            TypeExecutedOn = Type.GetType(group.First().job.TypeExecutedOn),
                            Status = group.First().job.Status,
                            JobArgs = group.OrderBy(p => p.param.ParamOrdinal)
                                .Select(s =>
                                    Newtonsoft.Json.JsonConvert.DeserializeObject(s.param.SerializedValue, Type.GetType(s.param.SerializedType, false))).ToArray(),
                            RetryParameters = new RetryParameters(group.First().job.MaxRetries, TimeSpan.FromSeconds(group.First().job.MinRetryWait), group.First().job.RetryCount, group.First().job.LastAttempt)
                        });
                
            }
        }

        public virtual void MarkJobInProgress(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
            {
                /*JobInProgressString =
                string.Format("update {0} set status='{1}', LockClaimTime=null, LastAttempt=getDate() where JobGuid = @jobGuid",
                    _tableConfig.QueueTableName,JobStates.InProgress);*/
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.Status, JobStates.InProgress)
                    .Set(q => q.LockClaimTime, (DateTime?) null)
                    .Set(q => q.LastAttempt, DateTime.Now)
                    .Update();
                //conn.Execute(JobInProgressString, new { jobGuid = jobId });
            }
        }

        

        public virtual void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
            {
                /*JobRetryIncrementString =
                string.Format(
                    "update {0} set status='{1}', RetryCount = RetryCount + 1, LastAttempt=getDate(), LockClaimTime=null where JobGuid = @jobGuid",
                    _tableConfig.QueueTableName, JobStates.Retry);*/
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.RetryCount, (current) => current.RetryCount + 1)
                    .Set(q => q.Status, JobStates.Retry)
                    .Set(q => q.LastAttempt, DateTime.Now)
                    .Set(q=>q.LockClaimTime, (DateTime?)null)
                    .Update();

            }
        }

        

        public virtual IOddJobWithMetadata GetJob(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDbConnection(_mappingSchema))
            {
                
                var jobWithParamQuery = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobId)
                    .InnerJoin(conn.GetTable<SqlCommonOddJobParamMetaData>()
                        , (job, param) => job.JobGuid == param.Id
                        , (job, param) => new {job, param}
                    );

                return jobWithParamQuery.ToList().GroupBy(q => q.job.JobGuid)
                    .Select(group =>
                        new SqlServerDbOddJob()
                        {
                            JobId = group.Key,
                            MethodName = group.First().job.MethodName,
                            TypeExecutedOn = Type.GetType(group.First().job.TypeExecutedOn),
                            Status = group.First().job.Status,
                            JobArgs = group.OrderBy(p => p.param.ParamOrdinal)
                                .Select(s =>
                                    Newtonsoft.Json.JsonConvert.DeserializeObject(s.param.SerializedValue, Type.GetType(s.param.SerializedType, false))).ToArray(),
                            RetryParameters = new RetryParameters(group.First().job.MaxRetries, TimeSpan.FromSeconds(group.First().job.MinRetryWait), group.First().job.RetryCount, group.First().job.LastAttempt)
                        }).FirstOrDefault();
                    }
        }

        
    }
}
