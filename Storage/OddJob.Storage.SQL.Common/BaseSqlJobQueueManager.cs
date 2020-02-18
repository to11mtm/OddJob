using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Linq;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    
   

    public class BaseSqlJobQueueManager : IJobQueueManager
    {
        protected readonly ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration;
        protected readonly  FluentMappingBuilder _mappingSchema;
        private readonly IJobTypeResolver _typeResolver;


        public BaseSqlJobQueueManager(IJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration, IJobTypeResolver typeResolver)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            
            _jobQueueTableConfiguration = jobQueueTableConfiguration;
            _typeResolver = typeResolver;

            _mappingSchema = MappingSchema.Default.GetFluentMappingBuilder();
        }

        public virtual async Task<IEnumerable<IOddJobWithMetadata>> GetJobsAsync(string[] queueNames,
            int fetchSize, Expression<Func<JobLockData, object>> orderPredicate,
            CancellationToken token = default(CancellationToken))
        {
            await SynchronizationContextManager.RemoveContext;
            token.ThrowIfCancellationRequested();
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var lockGuid = Guid.NewGuid();
                var updateCmd = LockUpdateQuery(fetchSize, orderPredicate, conn,
                    lockGuid, jobMetaData =>
                        queueNames.Contains(jobMetaData.QueueName) &&
                        (jobMetaData.Status == "New" ||
                         (jobMetaData.Status == "Retry")));
                var updateCount = await updateCmd.UpdateAsync(token);
                if (updateCount <= 0)
                {
                    return  new List<SqlDbOddJob>();
                }

                return await OddJobWithMetadatasAsync(conn, lockGuid, queueNames);
            }
        }

        

        public virtual IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames,
            int fetchSize, Expression<Func<JobLockData, object>> orderPredicate)
        {
            
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                //Because our Lock Update Does the lock, we don't bother with a transaction.
                var lockGuid = Guid.NewGuid();
                var updateCmd = LockUpdateQuery(fetchSize, orderPredicate, conn,
                    lockGuid, jobMetaData =>
                        queueNames.Contains(jobMetaData.QueueName) &&
                        (jobMetaData.Status == "New" ||
                         (jobMetaData.Status == "Retry")));


                var updateCount = updateCmd.Update();


                return updateCount > 0 ? OddJobWithMetadatas(conn, lockGuid, queueNames) : new List<SqlDbOddJob>() ;
            }
        }

         private IUpdatable<SqlCommonDbOddJobMetaData> LockUpdateQuery(int fetchSize, Expression<Func<JobLockData, object>> orderPredicate, DataConnection conn,
            Guid lockGuid, Expression<Func<SqlCommonDbOddJobMetaData, bool>> expression)
        {
            var lockTime = DateTime.Now;
            var lockClaimTimeoutThreshold = DateTime.Now.AddSeconds(
                (0) - _jobQueueTableConfiguration.JobClaimLockTimeoutInSeconds);
            var defaultMinCoalesce = DateTime.MinValue;
            IQueryable<JobLockData> lockingCheckQuery =
                QueueTable(conn)
                    .Where(jobMetaData=>
                        //jobMetaData => queueNames.Contains(jobMetaData.QueueName) 
                        //     &&
                             (jobMetaData.DoNotExecuteBefore <= lockTime || jobMetaData.DoNotExecuteBefore == null)
                             &&
                             (( jobMetaData.MaxRetries >= jobMetaData.RetryCount &&
                               (jobMetaData.LastAttempt.Value.AddSeconds(jobMetaData.MinRetryWait) <= lockTime)
                                 || jobMetaData.LastAttempt == null)
                             )
                             && (jobMetaData.LockClaimTime == null || jobMetaData.LockClaimTime <
                                 lockClaimTimeoutThreshold)
                             
                    ).Where(expression).Select(lockProjection => new JobLockData
                    {
                        JobId = lockProjection.Id,
                        MostRecentDate =
                            (lockProjection.CreatedDate ?? defaultMinCoalesce)
                            > (lockProjection.LastAttempt ?? defaultMinCoalesce)
                                ? lockProjection.CreatedDate
                                : lockProjection.LastAttempt,
                        CreatedDate = lockProjection.CreatedDate,
                        LastAttempt = lockProjection.LastAttempt,
                        Retries = lockProjection.RetryCount,
                        DoNotExecuteBefore = lockProjection.DoNotExecuteBefore,
                        Status = lockProjection.Status
                    }).OrderBy(orderPredicate).Take(fetchSize);
            var updateWhere = QueueTable(conn)
                .Where(q => lockingCheckQuery.Any(r => r.JobId == q.Id));
            var updateCmd = updateWhere.Set(q => q.LockGuid, lockGuid)
                .Set(q => q.LockClaimTime, lockTime);
            return updateCmd;
        }

        private IEnumerable<SqlDbOddJob> OddJobWithMetadatas(DataConnection conn, Guid lockGuid, string[] queueNames)
        {
            var jobWithParamQuery = QueueTable(conn)
                .Where(jobMetadata =>  jobMetadata.LockGuid == lockGuid);

            var resultSet = ExecuteJoinQuery(jobWithParamQuery, conn).ToList();


            return resultSet;
        }
        
        private async Task<IEnumerable<SqlDbOddJob>> OddJobWithMetadatasAsync(DataConnection conn, Guid lockGuid, string[] queueNames)
        {
            var jobWithParamQuery = QueueTable(conn)
                .Where(jobMetadata =>  jobMetadata.LockGuid == lockGuid);

            var resultSet =
                await ExecuteJoinQueryAsync(jobWithParamQuery, conn);


            return resultSet.ToList();
        }

        public virtual void MarkJobInProgress(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                QueueTable(conn)
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.Status, JobStates.InProgress)
                    .Set(q => q.LockClaimTime, (DateTime?)null)
                    .Set(q => q.LastAttempt, DateTime.Now)
                    .Update();

            }
        }
        
        public virtual async Task MarkJobInProgressAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            await SynchronizationContextManager.RemoveContext;
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                await QueueTable(conn)
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.Status, JobStates.InProgress)
                    .Set(q => q.LockClaimTime, (DateTime?)null)
                    .Set(q => q.LastAttempt, DateTime.Now)
                    .UpdateAsync(cancellationToken);

            }
        }

        public virtual void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                QueueTable(conn)
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.RetryCount, (current) => current.RetryCount + 1)
                    .Set(q => q.Status, JobStates.Retry)
                    .Set(q => q.LastAttempt, DateTime.Now)
                    .Set(q => q.LockClaimTime, (DateTime?)null)
                    .Update();

            }
        }
        
        public virtual async Task MarkJobInRetryAndIncrementAsync(Guid jobId, DateTime lastAttempt, CancellationToken cancellationToken = default)
        {
            await SynchronizationContextManager.RemoveContext;
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                await QueueTable(conn)
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.RetryCount, (current) => current.RetryCount + 1)
                    .Set(q => q.Status, JobStates.Retry)
                    .Set(q => q.LastAttempt, DateTime.Now)
                    .Set(q => q.LockClaimTime, (DateTime?)null)
                    .UpdateAsync(cancellationToken);

            }
        }
        

         

        public virtual IOddJobWithMetadata GetJob(Guid jobId, bool withLock, bool requiresValidStatus)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var canGet = 1;
                if (withLock)
                {
                    var lockGuid = Guid.NewGuid(); 
                    IUpdatable<SqlCommonDbOddJobMetaData> lockQ = null;
                     if (requiresValidStatus)
                     {
                         
                         lockQ = LockUpdateQuery(1, r => r.CreatedDate, conn,
                             lockGuid, r => r.JobGuid == jobId && (r.Status == "New"|| r.Status == "Retry" ));
                     }
                     else
                     {
                         lockQ = LockUpdateQuery(1, r => r.CreatedDate, conn,
                             lockGuid, r => r.JobGuid == jobId);
                     }
                    var lockTime = DateTime.Now;
                    var lockClaimTimeoutThreshold = DateTime.Now.AddSeconds(
                        (0) - _jobQueueTableConfiguration.JobClaimLockTimeoutInSeconds);
                    //canGet = updateSingleJobCmdQuery(requiresValidStatus) (conn, lockGuid, jobId, lockTime,
                    //    lockClaimTimeoutThreshold);
                    canGet = lockQ.Update();
                }

                if (canGet > 0)
                {
                    var jobWithParamQuery = QueueTable(conn)
                        .Where(jobMetadata => jobMetadata.JobGuid == jobId);

                    var resultSet = ExecuteJoinQuery(jobWithParamQuery, conn);
                    return resultSet.FirstOrDefault();
                }

                return null;
            }
        }
        
        public virtual async Task<IOddJobWithMetadata> GetJobAsync(Guid jobId, bool withLock,bool requiresValidStatus, CancellationToken cancellationToken = default)
        {
            await SynchronizationContextManager.RemoveContext;
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var canGet = 1;
                if (withLock)
                {
                    var lockGuid = Guid.NewGuid();
                    IUpdatable<SqlCommonDbOddJobMetaData> lockQ = null;
                    if (requiresValidStatus)
                    {
                        
                        lockQ = LockUpdateQuery(1, r => r.CreatedDate, conn,
                            lockGuid, r => r.JobGuid == jobId && (r.Status == "New"|| r.Status == "Retry" ));
                    }
                    else
                    {
                        lockQ = LockUpdateQuery(1, r => r.CreatedDate, conn,
                            lockGuid, r => r.JobGuid == jobId);
                    }

                    canGet = await lockQ.UpdateAsync(cancellationToken);
                }

                if (canGet > 0)
                {
                    var jobWithParamQuery = QueueTable(conn)
                        .Where(jobMetadata => jobMetadata.JobGuid == jobId);

                    var resultSet = await ExecuteJoinQueryAsync(jobWithParamQuery, conn, cancellationToken);
                    return resultSet.FirstOrDefault();
                }

                return null;
            }
        }

        public virtual void MarkJobSuccess(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                QueueTable(conn)
                    .Where(q => q.JobGuid == jobGuid)
                    .Set(q => q.Status, JobStates.Processed)
                    .Set(q => q.LockClaimTime, (DateTime?)null)
                    .Update();
            }
        }
        
        public virtual async Task MarkJobSuccessAsync(Guid jobGuid, CancellationToken cancellationToken = default)
        {
            await SynchronizationContextManager.RemoveContext;
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                await QueueTable(conn)
                    .Where(q => q.JobGuid == jobGuid)
                    .Set(q => q.Status, JobStates.Processed)
                    .Set(q => q.LockClaimTime, (DateTime?)null)
                    .UpdateAsync(cancellationToken);
            }
        }
        

        public virtual void MarkJobFailed(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                 QueueTable(conn)
                     .Where(q => q.JobGuid == jobGuid)
                     .Set(q => q.Status, JobStates.Failed)
                     .Set(q => q.LockClaimTime, (DateTime?)null)
                     .Update();

            }
        }
        
        public virtual async Task MarkJobFailedAsync(Guid jobGuid, CancellationToken cancellationToken = default)
        {
            await SynchronizationContextManager.RemoveContext;
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                await QueueTable(conn)
                    .Where(q => q.JobGuid == jobGuid)
                    .Set(q => q.Status, JobStates.Failed)
                    .Set(q => q.LockClaimTime, (DateTime?)null)
                    .UpdateAsync(cancellationToken);

            }
        }
        protected IJobQueueDataConnectionFactory _jobQueueConnectionFactory { get; private set; }


        protected ITable<SqlCommonOddJobParamMetaData> ParamTable(DataConnection conn)
        {
            return conn.GetTable<SqlCommonOddJobParamMetaData>().TableName(_jobQueueTableConfiguration.ParamTableName);
        }

        protected ITable<SqlCommonDbOddJobMetaData> QueueTable(DataConnection conn)
        {
            return conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(_jobQueueTableConfiguration.QueueTableName);
        }
        protected ITable<SqlDbOddJobMethodGenericInfo> MethodGenericParameterTable(DataConnection conn)
        {
            return conn.GetTable<SqlDbOddJobMethodGenericInfo>().TableName(_jobQueueTableConfiguration.JobMethodGenericParamTableName);
        }


        protected IEnumerable<SerializableOddJob> ExecuteSerializableJoinQuery(IQueryable<SqlCommonDbOddJobMetaData> jobWithParamQuery, DataConnection conn)
        {
            var newQuery = jobWithParamQuery.LeftJoin(ParamTable(conn)
                , (job, param) => job.JobGuid == param.JobGuid
                , (job, param) => new SqlQueueRowSet() {MetaData = job, ParamData = param}
            ).LeftJoin(MethodGenericParameterTable(conn)
                , (job_param, jobGeneric) => job_param.MetaData.JobGuid == jobGeneric.JobGuid
                , (job_param, jobGeneric) => new {MetaData = job_param.MetaData, ParamData = job_param.ParamData, JobMethodGenericData = jobGeneric});
            var resultSet = newQuery.ToList();
            var finalSet = resultSet.GroupBy(q => q.MetaData.JobGuid)
                .Select(group =>
                    new SerializableOddJob()
                    {
                        JobId = group.Key,
                        MethodName = group.First().MetaData.MethodName,
                        TypeExecutedOn = group.First().MetaData.TypeExecutedOn,
                        Status = group.First().MetaData.Status,
                        ExecutionTime = group.First().MetaData.DoNotExecuteBefore,
                        QueueName = group.First().MetaData.QueueName,
                        JobArgs = group.Where(r=>r.ParamData!=null)
                            .OrderBy(p => p.ParamData.ParamOrdinal) //Order by for Reader paranoia
                            .Select(param => param.ParamData)
                            .Where(s => s.SerializedType != null)
                            .GroupBy(param => param.ParamOrdinal)
                            .Select(s => new OddJobSerializedParameter()
                            {
                                Ordinal = s.FirstOrDefault().ParamOrdinal,
                                Name = s.FirstOrDefault().ParameterName,
                                Value = s.FirstOrDefault().SerializedValue,
                                TypeName    = s.FirstOrDefault().SerializedType
                            }).ToArray(),
                        RetryParameters = new RetryParameters(group.First().MetaData.MaxRetries,
                            TimeSpan.FromSeconds(group.First().MetaData.MinRetryWait),
                            group.First().MetaData.RetryCount, group.First().MetaData.LastAttempt),
                        MethodGenericTypes = group.Where(r=>r.JobMethodGenericData != null).OrderBy(q => q.JobMethodGenericData.ParamOrder) //Order by for reader paranoia.
                            .Where(t => t.JobMethodGenericData.ParamTypeName != null)
                            .Select(q => q.JobMethodGenericData)
                            .GroupBy(q => q.ParamOrder)
                            .Select(t => t.FirstOrDefault().ParamTypeName).ToArray()
                    });
            return finalSet;
        }

        protected IEnumerable<SqlDbOddJob> ExecuteJoinQuery(IQueryable<SqlCommonDbOddJobMetaData> jobWithParamQuery, DataConnection conn)
        {
            var newQuery = jobWithParamQuery.LeftJoin(ParamTable(conn)
                , (job, param) => job.JobGuid == param.JobGuid
                , (job, param) => new SqlQueueRowSet() {MetaData = job, ParamData = param}
            ).LeftJoin(MethodGenericParameterTable(conn)
            , (job_param,jobGeneric)=> job_param.MetaData.JobGuid == jobGeneric.JobGuid
            , (job_param,jobGeneric)=> new{MetaData = job_param.MetaData, ParamData = job_param.ParamData, JobMethodGenericData = jobGeneric});
            
            //We do the result set this way for a few reasons:
            // - In most sane usages, there shouldn't be a lot of data coming back
            // - Linq2Db does a good job of making the right choices
            // This ensures we properly fetch form DB before passing back.
            var resultSet = newQuery.ToList();
            
            var finalSet =  resultSet.GroupBy(jobMetadata => jobMetadata.MetaData.JobGuid)
                .Select(group =>
                    new SqlDbOddJob()
                    {
                        JobId = group.Key,
                        MethodName = group.First().MetaData.MethodName,
                        TypeExecutedOn = _typeResolver.GetTypeForJob(group.First().MetaData.TypeExecutedOn),
                        Status = group.First().MetaData.Status,
                        ExecutionTime = group.First().MetaData.DoNotExecuteBefore,
                        Queue= group.First().MetaData.QueueName,
                        JobArgs = group.OrderBy(p => p.ParamData?.ParamOrdinal) //Order by for Reader paranoia
                            .Select(q=>q.ParamData).Where(s => s?.SerializedType != null).GroupBy(q=>q.ParamOrdinal)
                            .Select(s => new OddJobParameter() { Name = s.FirstOrDefault()?.ParameterName, Value = 
                                Newtonsoft.Json.JsonConvert.DeserializeObject(s.FirstOrDefault()?.SerializedValue,
                                    _typeResolver.GetTypeForJob(s.FirstOrDefault()?.SerializedType), SerializableJobCreator.Settings)
                                , Type= TargetPlatformHelpers.ReplaceCoreTypes(s.FirstOrDefault()?.SerializedType)
                            }).ToArray(),
                        RetryParameters = new RetryParameters(group.First().MetaData.MaxRetries,
                            TimeSpan.FromSeconds(group.First().MetaData.MinRetryWait),
                            group.First().MetaData.RetryCount, group.First().MetaData.LastAttempt),
                        MethodGenericTypes = group.OrderBy(q=>q.JobMethodGenericData?.ParamOrder) //Order by for reader paranoia.
                            .Where(t=>t.JobMethodGenericData!= null && t.JobMethodGenericData.ParamTypeName != null)
                            .Select(q=>q.JobMethodGenericData)
                            .GroupBy(q=>q.ParamOrder)
                            .Select(t=> _typeResolver.GetTypeForJob(t.FirstOrDefault().ParamTypeName)).ToArray(),
                    });
            return finalSet;
        }
        
        protected async Task<IEnumerable<SqlDbOddJob>> ExecuteJoinQueryAsync(IQueryable<SqlCommonDbOddJobMetaData> jobWithParamQuery, DataConnection conn, CancellationToken cancellationToken = default)
        {
            var newQuery = jobWithParamQuery.LeftJoin(ParamTable(conn)
                , (job, param) => job.JobGuid == param.JobGuid
                , (job, param) => new SqlQueueRowSet() {MetaData = job, ParamData = param}
            ).LeftJoin(MethodGenericParameterTable(conn)
            , (job_param,jobGeneric)=> job_param.MetaData.JobGuid == jobGeneric.JobGuid
            , (job_param,jobGeneric)=> new{MetaData = job_param.MetaData, ParamData = job_param.ParamData, JobMethodGenericData = jobGeneric});
            
            //We do the result set this way for a few reasons:
            // - In most sane usages, there shouldn't be a lot of data coming back
            // - Linq2Db does a good job of making the right choices
            var resultSet = await newQuery.ToListAsync(cancellationToken);
            
            var finalSet =  resultSet.GroupBy(jobMetadata => jobMetadata.MetaData.JobGuid)
                .Select(group =>
                    new SqlDbOddJob()
                    {
                        JobId = group.Key,
                        MethodName = group.First().MetaData.MethodName,
                        TypeExecutedOn = _typeResolver.GetTypeForJob(group.First().MetaData.TypeExecutedOn),
                        Status = group.First().MetaData.Status,
                        ExecutionTime = group.First().MetaData.DoNotExecuteBefore,
                        Queue= group.First().MetaData.QueueName,
                        JobArgs = group.OrderBy(p => p.ParamData?.ParamOrdinal) //Order by for Reader paranoia
                            .Select(q=>q.ParamData).Where(s => s?.SerializedType != null).GroupBy(q=>q.ParamOrdinal)
                            .Select(s => new OddJobParameter() { Name = s.FirstOrDefault()?.ParameterName, Value = 
                                Newtonsoft.Json.JsonConvert.DeserializeObject(s.FirstOrDefault()?.SerializedValue,
                                    _typeResolver.GetTypeForJob(s.FirstOrDefault()?.SerializedType), SerializableJobCreator.Settings)
                                , Type= TargetPlatformHelpers.ReplaceCoreTypes(s.FirstOrDefault()?.SerializedType)
                            }).ToArray(),
                        RetryParameters = new RetryParameters(group.First().MetaData.MaxRetries,
                            TimeSpan.FromSeconds(group.First().MetaData.MinRetryWait),
                            group.First().MetaData.RetryCount, group.First().MetaData.LastAttempt),
                        MethodGenericTypes = group.OrderBy(q=>q.JobMethodGenericData?.ParamOrder) //Order by for reader paranoia.
                            .Where(t=>t.JobMethodGenericData!= null && t.JobMethodGenericData.ParamTypeName != null)
                            .Select(q=>q.JobMethodGenericData)
                            .GroupBy(q=>q.ParamOrder)
                            .Select(t=> _typeResolver.GetTypeForJob(t.FirstOrDefault().ParamTypeName)).ToArray(),
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
