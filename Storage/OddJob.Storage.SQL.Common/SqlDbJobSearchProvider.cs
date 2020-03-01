using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using LinqToDB;
using LinqToDB.Linq;
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
}