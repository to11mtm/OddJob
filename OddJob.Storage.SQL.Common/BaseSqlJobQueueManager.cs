using System;
using System.Collections.Generic;
using System.Linq;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
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
        private readonly ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration;
        private readonly  MappingSchema _mappingSchema;
        private readonly IStorageJobTypeResolver _typeResolver;

        protected BaseSqlJobQueueManager(IJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration):this(jobQueueConnectionFactory,jobQueueTableConfiguration, new NullOnMissingTypeJobTypeResolver())
        {

        }
        protected BaseSqlJobQueueManager(IJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration, IStorageJobTypeResolver typeResolver)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            
            _jobQueueTableConfiguration = jobQueueTableConfiguration;
            _typeResolver = typeResolver;

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
            var lockClaimTimeoutThreshold = DateTime.Now.AddSeconds(
                (0) - _jobQueueTableConfiguration.JobClaimLockTimeoutInSeconds);
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema))
            {
                //Because our Lock Update Does the lock, we don't bother with a transaction.

                IQueryable<JobLockData> lockingCheckQuery =
                    conn.GetTable<SqlCommonDbOddJobMetaData>()
                        .Where(
                            q => queueNames.Contains(q.QueueName)
                                 &&
                                 (q.DoNotExecuteBefore <= lockTime || q.DoNotExecuteBefore == null)
                                 &&
                                 (q.Status == "New" ||
                                  (q.Status == "Retry" && q.MaxRetries >= q.RetryCount &&
                                   q.LastAttempt.Value.AddSeconds(q.MinRetryWait) <=lockTime)

                                 )
                                 && (q.LockClaimTime == null || q.LockClaimTime <
                                    lockClaimTimeoutThreshold)
                        ).Select(q => new JobLockData
                        {
                            JobId = q.Id,
                            MostRecentDate =
                                (q.CreatedDate ?? DateTime.MinValue)
                                > (q.LastAttempt ?? DateTime.MinValue)
                                    ? q.CreatedDate
                                    : q.LastAttempt
                        }).OrderBy(q => q.MostRecentDate).Take(fetchSize);
                var updateWhere = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => lockingCheckQuery.Any(r => r.JobId == q.Id));
                    var updateCmd = updateWhere.Set(q => q.LockGuid, lockGuid)
                    .Set(q => q.LockClaimTime, lockTime);
                    updateCmd.Update();
                        
                
                        
                var jobWithParamQuery = conn.GetTable<SqlCommonDbOddJobMetaData>()
                    .Where(q => q.LockGuid == lockGuid);

                var resultSet = ExecuteJoinQuery(jobWithParamQuery, conn);


                return resultSet;

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
                    .Where(q => q.JobGuid == jobId);

                var resultSet = ExecuteJoinQuery(jobWithParamQuery, conn);
                return resultSet.FirstOrDefault();
            }
        }

        private IEnumerable<SqlDbOddJob> ExecuteJoinQuery(IQueryable<SqlCommonDbOddJobMetaData> jobWithParamQuery, DataConnection conn)
        {
            var newQuery = jobWithParamQuery.LeftJoin(conn.GetTable<SqlCommonOddJobParamMetaData>()
                , (job, param) => job.JobGuid == param.JobGuid
                , (job, param) => new SqlQueueRowSet() {MetaData = job, ParamData = param}
            ).LeftJoin(conn.GetTable<SqlDbOddJobMethodGenericInfo>()
            , (job_param,jobGeneric)=> job_param.MetaData.JobGuid == jobGeneric.JobGuid
            , (job_param,jobGeneric)=> new{MetaData = job_param.MetaData, ParamData = job_param.ParamData, JobMethodGenericData = jobGeneric});
            var resultSet = newQuery.ToList();
            var finalSet =  resultSet.GroupBy(q => q.MetaData.JobGuid)
                .Select(group =>
                    new SqlDbOddJob()
                    {
                        JobId = group.Key,
                        MethodName = group.First().MetaData.MethodName,
                        TypeExecutedOn = _typeResolver.GetTypeForJob(group.First().MetaData.TypeExecutedOn),
                        Status = group.First().MetaData.Status,
                        JobArgs = group.OrderBy(p => p.ParamData.ParamOrdinal).Select(q=>q.ParamData).Where(s => s.SerializedType != null).GroupBy(q=>q.ParamOrdinal)
                            .Select(s =>
                                Newtonsoft.Json.JsonConvert.DeserializeObject(s.FirstOrDefault().SerializedValue,
                                    Type.GetType(s.FirstOrDefault().SerializedType, false))).ToArray(),
                        RetryParameters = new RetryParameters(group.First().MetaData.MaxRetries,
                            TimeSpan.FromSeconds(group.First().MetaData.MinRetryWait),
                            group.First().MetaData.RetryCount, group.First().MetaData.LastAttempt),
                        MethodGenericTypes = group.OrderBy(q=>q.JobMethodGenericData.ParamOrder).Where(t=>t.JobMethodGenericData!= null && t.JobMethodGenericData.ParamTypeName != null)
                            .Select(q=>q.JobMethodGenericData)
                            .GroupBy(q=>q.ParamOrder)
                            .Select(t=> Type.GetType(t.FirstOrDefault().ParamTypeName)).ToArray()
                        
                    });
            return finalSet;
        }
    }

    class SqlQueueRowSet
    {
        public SqlCommonDbOddJobMetaData MetaData { get; set; }
        public SqlCommonOddJobParamMetaData ParamData { get; set; }
    }
}
