using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Linq;
using LinqToDB.Mapping;
using LinqToDB.Tools;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    

    public abstract class BaseSqlJobQueueManager : IJobQueueManager , IJobSearchProvider
    {
        private readonly ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration;
        private readonly  FluentMappingBuilder _mappingSchema;
        private readonly IJobTypeResolver _typeResolver;


        protected BaseSqlJobQueueManager(IJobQueueDataConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration, IJobTypeResolver typeResolver)
        {
            _jobQueueConnectionFactory = jobQueueConnectionFactory;
            
            _jobQueueTableConfiguration = jobQueueTableConfiguration;
            _typeResolver = typeResolver;

            _mappingSchema = MappingSchema.Default.GetFluentMappingBuilder();
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
        public virtual void MarkJobSuccess(Guid jobGuid)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                
                QueueTable(conn)
                    .Where(q => q.JobGuid == jobGuid)
                    .Set(q => q.Status, JobStates.Processed)
                    .Set(q => q.LockClaimTime, (DateTime?) null)
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
                    .Set(q => q.LockClaimTime,(DateTime?)null)
                    .Update();

            }
        }

        public IEnumerable<IOddJobWithMetadata> GetJobsByCriteria(Expression<Func<SqlCommonDbOddJobMetaData, bool>> criteria)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var criteriaQuery = QueueTable(conn).Where(criteria);
                var resultSet = ExecuteJoinQuery(criteriaQuery, conn);
                return resultSet;
            }

        }

        public IEnumerable<IOddJobWithMetadata> GetJobsByParameterAndMainCriteria(
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> jobQueryable, Expression<Func<SqlCommonOddJobParamMetaData, bool>> paramQueryable)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                var criteria = QueueTable(conn)
                    .Where(q => q.JobGuid.In(
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
                    .Select(selector).Distinct();
            }
        }
        
        public IEnumerable<T> GetJobParamCriteriaValues<T>(Expression<Func<SqlCommonOddJobParamMetaData, T>> selector)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                return ParamTable(conn)
                    .Select(selector).Distinct();
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




        public virtual IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize, Expression<Func<JobLockData, object>> orderPredicate)
        {
            var lockGuid = Guid.NewGuid();
            var lockTime = DateTime.Now;
            var lockClaimTimeoutThreshold = DateTime.Now.AddSeconds(
                (0) - _jobQueueTableConfiguration.JobClaimLockTimeoutInSeconds);
            var defaultMinCoalesce = DateTime.MinValue;
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                //Because our Lock Update Does the lock, we don't bother with a transaction.

                IQueryable<JobLockData> lockingCheckQuery =
                    QueueTable(conn)
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
                                (q.CreatedDate ?? defaultMinCoalesce)
                                > (q.LastAttempt ?? defaultMinCoalesce)
                                    ? q.CreatedDate
                                    : q.LastAttempt,
                            CreatedDate = q.CreatedDate,
                            LastAttempt = q.LastAttempt,
                            Retries = q.RetryCount,
                            DoNotExecuteBefore = q.DoNotExecuteBefore,
                            Status = q.Status
                        }).OrderBy(orderPredicate).Take(fetchSize);
                var updateWhere = QueueTable(conn)
                    .Where(q => lockingCheckQuery.Any(r => r.JobId == q.Id));
                    var updateCmd = updateWhere.Set(q => q.LockGuid, lockGuid)
                    .Set(q => q.LockClaimTime, lockTime);
                    updateCmd.Update();
                        
                
                        
                var jobWithParamQuery = QueueTable(conn)
                    .Where(q => q.LockGuid == lockGuid);

                var resultSet = ExecuteJoinQuery(jobWithParamQuery, conn).ToList();

                
                return resultSet;

            }
        }

        public virtual void MarkJobInProgress(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                QueueTable(conn)
                    .Where(q => q.JobGuid == jobId)
                    .Set(q => q.Status, JobStates.InProgress)
                    .Set(q => q.LockClaimTime, (DateTime?) null)
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
                    .Set(q=>q.LockClaimTime, (DateTime?)null)
                    .Update();

            }
        }


        public virtual IOddJobWithMetadata GetJob(Guid jobId)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {

                var jobWithParamQuery = QueueTable(conn)
                    .Where(q => q.JobGuid == jobId);

                var resultSet = ExecuteJoinQuery(jobWithParamQuery, conn);
                return resultSet.FirstOrDefault();
            }
        }

        private IEnumerable<SqlDbOddJob> ExecuteJoinQuery(IQueryable<SqlCommonDbOddJobMetaData> jobWithParamQuery, DataConnection conn)
        {
            var newQuery = jobWithParamQuery.LeftJoin(ParamTable(conn)
                , (job, param) => job.JobGuid == param.JobGuid
                , (job, param) => new SqlQueueRowSet() {MetaData = job, ParamData = param}
            ).LeftJoin(MethodGenericParameterTable(conn)
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
                        ExecutionTime = group.First().MetaData.DoNotExecuteBefore,
                        Queue= group.First().MetaData.QueueName,
                        JobArgs = group.OrderBy(p => p.ParamData.ParamOrdinal) //Order by for Reader paranoia
                            .Select(q=>q.ParamData).Where(s => s.SerializedType != null).GroupBy(q=>q.ParamOrdinal)
                            .Select(s => new OddJobParameter() { Name = s.FirstOrDefault().ParameterName, Value = 
                                Newtonsoft.Json.JsonConvert.DeserializeObject(s.FirstOrDefault().SerializedValue,
                                    Type.GetType(TargetPlatformHelpers.ReplaceCoreTypes(s.FirstOrDefault().SerializedType), false))}).ToArray(),
                        RetryParameters = new RetryParameters(group.First().MetaData.MaxRetries,
                            TimeSpan.FromSeconds(group.First().MetaData.MinRetryWait),
                            group.First().MetaData.RetryCount, group.First().MetaData.LastAttempt),
                        MethodGenericTypes = group.OrderBy(q=>q.JobMethodGenericData.ParamOrder) //Order by for reader paranoia.
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
