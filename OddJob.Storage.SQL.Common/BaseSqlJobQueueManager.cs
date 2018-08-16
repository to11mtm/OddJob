using System;
using System.Collections.Generic;
using System.Linq;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
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
        private readonly ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration;
        private readonly  MappingSchema _mappingSchema;

        protected BaseSqlJobQueueManager(IJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            
            _jobQueueTableConfiguration = jobQueueTableConfiguration;


            _mappingSchema = Mapping.BuildMappingSchema(jobQueueTableConfiguration);
        }

        protected IJobQueueDataConnectionFactory _jobQueueConnectionFactory { get; private set; }
        

        public virtual void MarkJobSuccess(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobGuid)
                    .Set(q => q.Status, JobStates.Processed)
                    .Set(q => q.LockClaimTime, (DateTime?) null)
                    .Update();
            }
        }


        public virtual void MarkJobFailed(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
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
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                //Because our Lock Update Does the lock, we don't bother with a transaction.

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
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => lockingCheckQuery.Any(r => r.JobId == q.Id))
                    .Set(q => q.LockGuid, lockGuid)
                    .Set(q => q.LockClaimTime, lockTime)
                    .Update();
                        
                
                        
                var jobWithParamQuery = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.LockGuid == lockGuid)
                    .InnerJoin(conn.GetTable<SqlCommonOddJobParamMetaData>()
                        , (job, param) => job.JobGuid == param.JobGuid
                        , (job, param) => new {job, param});
                
                return jobWithParamQuery.ToList().GroupBy(q=>q.job.JobGuid)
                    .Select(group =>
                        new SqlDbOddJob()
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
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                
                conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.Status, JobStates.InProgress)
                    .Set(q => q.LockClaimTime, (DateTime?) null)
                    .Set(q => q.LastAttempt, DateTime.Now)
                    .Update();
                
            }
        }

        

        public virtual void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                
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
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                
                var jobWithParamQuery = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.JobGuid == jobId)
                    .InnerJoin(conn.GetTable<SqlCommonOddJobParamMetaData>()
                        , (job, param) => job.JobGuid == param.JobGuid
                        , (job, param) => new {job, param}
                    );

                return jobWithParamQuery.ToList().GroupBy(q => q.job.JobGuid)
                    .Select(group =>
                        new SqlDbOddJob()
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
