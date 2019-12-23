using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Linq;
using LinqToDB.Mapping;
using LinqToDB.Tools;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    public class SqlDbJobSearchProvider : BaseSqlJobQueueManager, IJobSearchProvider
    {
        public SqlDbJobSearchProvider(IJobQueueDataConnectionFactory jobQueueConnectionFactory, ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration, IJobTypeResolver typeResolver) : base(jobQueueConnectionFactory, jobQueueTableConfiguration, typeResolver)
        {
        }

        

        public IEnumerable<SerializableOddJob> GetSerializableJobsByCriteria(
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> criteria)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var criteriaQuery = QueueTable(conn).Where(criteria);
                var resultSet = ExecuteSerializableJoinQuery(criteriaQuery, conn);
                return resultSet.ToList();
            }
        }

        public IEnumerable<IOddJobWithMetadata> GetJobsByCriteria(Expression<Func<SqlCommonDbOddJobMetaData, bool>> criteria)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var criteriaQuery = QueueTable(conn).Where(criteria);
                var resultSet = ExecuteJoinQuery(criteriaQuery, conn);
                return resultSet.ToList();
            }

        }

        public IEnumerable<IOddJobWithMetadata> GetJobsByParameterAndMainCriteria(
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> jobQueryable, Expression<Func<SqlCommonOddJobParamMetaData, bool>> paramQueryable)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                var criteria = QueueTable(conn)
                    .Where(jobMetadata => jobMetadata.JobGuid.In(
                        ParamTable(conn).Where(paramQueryable)
                            .Select(r => r.JobGuid)))
                    .Where(jobQueryable);
                var resultSet = ExecuteJoinQuery(criteria, conn);
                return resultSet;
            }
        }

        public IEnumerable<T> GetJobCriteriaValues<T>(Expression<Func<SqlCommonDbOddJobMetaData, T>> selector)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                return QueueTable(conn)
                    .Select(selector).Distinct().ToList();
            }
        }

        public IEnumerable<T> GetJobCriteriaByCriteria<T>(Expression<Func<SqlCommonDbOddJobMetaData, bool>> criteria, Expression<Func<SqlCommonDbOddJobMetaData, T>> selector)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var criteriaQuery = QueueTable(conn).Where(criteria);
                var resultSet = criteriaQuery.Select(selector);
                return resultSet.ToList();
            }

        }

        public IEnumerable<T> GetJobParamCriteriaValues<T>(Expression<Func<SqlCommonOddJobParamMetaData, T>> selector)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                return ParamTable(conn)
                    .Select(selector).Distinct().ToList();
            }
        }

        public bool UpdateJobParameterValues(IEnumerable<SqlCommonOddJobParamMetaData> metaDatas)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                foreach (var metaData in metaDatas)
                {
                    ParamTable(conn)
                        .Where(q => q.JobGuid == metaData.JobGuid && q.ParamOrdinal == metaData.ParamOrdinal)
                        .Set(q => q.SerializedType, metaData.SerializedType)
                        .Set(q => q.SerializedValue, metaData.SerializedValue)
                        .Update();
                }
            }

            return true;
        }

        /// <summary>
        /// Update Job Metadata and Parameters via a Built command.
        /// </summary>
        /// <param name="commandData">A <see cref="JobUpdateCommand"/> with Criteria for updating.</param>
        /// <returns></returns>
        /// <remarks>This is designed to be a 'safe-ish' update. If done in a <see cref="TransactionScope"/>,
        /// You can roll-back if this returns false. If no <see cref="TransactionScope"/> is active,
        /// you could get dirty writes under some very edgy cases.
        /// </remarks>
        public bool UpdateJobMetadataAndParameters(JobUpdateCommand commandData)
        {
            //TODO: Make this even safer.
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                bool ableToUpdateJob = true;
                var updatable = QueueTable(conn)
                    .Where(q => q.JobGuid == commandData.JobGuid);
                if (String.IsNullOrWhiteSpace(commandData.OldStatusIfRequired) == false)
                {
                    updatable = updatable.Where(q => q.Status == commandData.OldStatusIfRequired);
                }

                IUpdatable<SqlCommonDbOddJobMetaData> updateCommand = null;
                foreach (var set in commandData.SetJobMetadata)
                {
                    updateCommand = (updateCommand == null)
                        ? updatable.Set(set.Key, set.Value)
                        : updateCommand.Set(set.Key, set.Value);
                }
                if (updateCommand != null)
                {
                    ableToUpdateJob = updateCommand.Update() > 0;
                }
                else
                {
                    ableToUpdateJob = updatable.Any();
                }

                if (ableToUpdateJob && commandData.SetJobParameters != null && commandData.SetJobParameters.Count > 0)
                {
                    foreach (var jobParameter in commandData.SetJobParameters)
                    {
                        var updatableParam = ParamTable(conn)
                            .Where(q => q.JobGuid == commandData.JobGuid && q.ParamOrdinal == jobParameter.Key
                                                                         && ableToUpdateJob);
                        IUpdatable<SqlCommonOddJobParamMetaData> updateParamCommand = null;
                        foreach (var updatePair in jobParameter.Value)
                        {
                            updateParamCommand = (updateParamCommand == null)
                                ? updatableParam.Set(updatePair.Key, updatePair.Value)
                                : updateParamCommand.Set(updatePair.Key, updatePair.Value);
                        }

                        if (updateParamCommand != null)
                        {
                            var updatedRows = updateParamCommand.Update();
                            if (updatedRows == 0)
                            {
                                return false;
                            }
                        }
                    }
                }

                return ableToUpdateJob;
            }
        }

        public bool UpdateJobMetadataValues(
            IDictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object> setters, Guid jobGuid,
            string oldStatusIfRequired)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var updatable = QueueTable(conn)
                    .Where(q => q.JobGuid == jobGuid);
                if (String.IsNullOrWhiteSpace(oldStatusIfRequired) == false)
                {
                    updatable = updatable.Where(q => q.Status == oldStatusIfRequired);
                }

                IUpdatable<SqlCommonDbOddJobMetaData> updateCommand = null;
                foreach (var set in setters)
                {
                    updateCommand = (updateCommand == null)
                        ? updatable.Set(set.Key, set.Value)
                        : updateCommand.Set(set.Key, set.Value);
                }
                if (updateCommand != null)
                {
                    return updateCommand.Update() > 0;
                }

                return false;
            }
        }

        public bool UpdateJobMetadataFull(SqlCommonDbOddJobMetaData metaDataToUpdate, string oldStatusIfRequired)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var updatable = QueueTable(conn)
                    .Where(q => q.JobGuid == metaDataToUpdate.JobGuid);
                if (String.IsNullOrWhiteSpace(oldStatusIfRequired) == false)
                {
                    updatable = updatable.Where(q => q.Status == oldStatusIfRequired);
                }
                
                updatable.Set(q => q.Status, metaDataToUpdate.Status)
                    .Set(q => q.DoNotExecuteBefore, metaDataToUpdate.DoNotExecuteBefore)
                    .Set(q => q.LockGuid, metaDataToUpdate.LockGuid)
                    .Set(q => q.LockClaimTime, metaDataToUpdate.LockClaimTime)
                    .Set(q => q.MinRetryWait, metaDataToUpdate.MinRetryWait)
                    .Set(q => q.MethodName, metaDataToUpdate.MethodName)
                    .Set(q => q.TypeExecutedOn, metaDataToUpdate.TypeExecutedOn)
                    .Set(q => q.RetryCount, metaDataToUpdate.RetryCount)
                    .Set(q => q.MaxRetries, metaDataToUpdate.MaxRetries)
                    .Update();
            }

            return true;
        }

        

    }
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
            token.ThrowIfCancellationRequested();
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var lockGuid = Guid.NewGuid();
                var updateCmd = LockUpdateQuery(queueNames, fetchSize, orderPredicate, conn, lockGuid);
                var updateCount = await updateCmd.UpdateAsync(token);
                if (updateCount > 0)
                {
                    return  new List<SqlDbOddJob>();
                }

                return OddJobWithMetadatas(conn, lockGuid, queueNames);
            }
        }

        

        public virtual IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames,
            int fetchSize, Expression<Func<JobLockData, object>> orderPredicate)
        {
            
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                //Because our Lock Update Does the lock, we don't bother with a transaction.
                var lockGuid = Guid.NewGuid();
                var updateCmd = LockUpdateQuery(queueNames, fetchSize, orderPredicate, conn, lockGuid);


                var updateCount = updateCmd.Update();


                return updateCount > 0 ? OddJobWithMetadatas(conn, lockGuid, queueNames) : new List<SqlDbOddJob>() ;
            }
        }

        private IUpdatable<SqlCommonDbOddJobMetaData> LockUpdateQuery(string[] queueNames, int fetchSize, Expression<Func<JobLockData, object>> orderPredicate, DataConnection conn,
            Guid lockGuid)
        {
            var lockTime = DateTime.Now;
            var lockClaimTimeoutThreshold = DateTime.Now.AddSeconds(
                (0) - _jobQueueTableConfiguration.JobClaimLockTimeoutInSeconds);
            var defaultMinCoalesce = DateTime.MinValue;
            IQueryable<JobLockData> lockingCheckQuery =
                QueueTable(conn)
                    .Where(
                        jobMetaData => queueNames.Contains(jobMetaData.QueueName)
                             &&
                             (jobMetaData.DoNotExecuteBefore <= lockTime || jobMetaData.DoNotExecuteBefore == null)
                             &&
                             (jobMetaData.Status == "New" ||
                              (jobMetaData.Status == "Retry" && jobMetaData.MaxRetries >= jobMetaData.RetryCount &&
                               jobMetaData.LastAttempt.Value.AddSeconds(jobMetaData.MinRetryWait) <= lockTime)
                             )
                             && (jobMetaData.LockClaimTime == null || jobMetaData.LockClaimTime <
                                 lockClaimTimeoutThreshold)
                    ).Select(lockProjection => new JobLockData
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

        public virtual IOddJobWithMetadata GetJob(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                var jobWithParamQuery = QueueTable(conn)
                    .Where(jobMetadata => jobMetadata.JobGuid == jobId);

                var resultSet = ExecuteJoinQuery(jobWithParamQuery, conn);
                return resultSet.FirstOrDefault();
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
                        JobArgs = group.OrderBy(p => p.ParamData.ParamOrdinal) //Order by for Reader paranoia
                            .Select(param => param.ParamData).Where(s => s.SerializedType != null).GroupBy(param => param.ParamOrdinal)
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
                        MethodGenericTypes = group.OrderBy(q => q.JobMethodGenericData.ParamOrder) //Order by for reader paranoia.
                            .Where(t => t.JobMethodGenericData != null && t.JobMethodGenericData.ParamTypeName != null)
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
                                    _typeResolver.GetTypeForJob(s.FirstOrDefault()?.SerializedType))
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
