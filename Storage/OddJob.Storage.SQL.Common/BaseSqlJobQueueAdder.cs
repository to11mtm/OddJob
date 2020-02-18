using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    public class BaseSqlJobQueueAdder : IJobQueueAdder, ISerializedJobQueueAdder
    {
        private readonly FluentMappingBuilder _mappingSchema;
        private readonly IJobAdderQueueTableResolver _tableResolver;

        public BaseSqlJobQueueAdder(IJobQueueDataConnectionFactory jobQueueDataConnectionFactory,
            IJobAdderQueueTableResolver tableResolver)
        {
            _jobQueueConnectionFactory = jobQueueDataConnectionFactory;

            _mappingSchema = MappingSchema.Default.GetFluentMappingBuilder();
            _tableResolver = tableResolver;
        }



        private readonly IJobQueueDataConnectionFactory _jobQueueConnectionFactory;


        public virtual void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                foreach (var job in jobDataSet)
                {
                    _addJobImpl(job, conn);
                }
            }
        }
        
        public virtual async Task AddJobsAsync(IEnumerable<SerializableOddJob> jobDataSet, CancellationToken cancellationToken = default)
        {
            await SynchronizationContextManager.RemoveContext;
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                foreach (var job in jobDataSet)
                {
                    await _addJobImplAsync(job, conn, cancellationToken);
                }
            }
        }

        public virtual void AddJob(SerializableOddJob jobData)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                _addJobImpl(jobData, conn);
            }

        }

        public async Task AddJobAsync(SerializableOddJob jobData,
            CancellationToken cancellationToken = default)
        {
            await SynchronizationContextManager.RemoveContext;
            using (var conn =
                _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema
                    .MappingSchema))
            {
                await _addJobImplAsync(jobData, conn, cancellationToken);
            }
        }

        /*
        public void AddJob_Idempotent(SerializableOddJob jobData, DataConnection conn)
        {
            var mergeSource1 = new[] {GetMetaDataForJob(jobData)};
            var mergeSource2 = GetParamDataForJob(jobData);
            var mergeSource3 = GetGenericDataForJob(jobData);
            var table = _tableResolver.GetConfigurationForJob(jobData);
            conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(table.QueueTableName).Merge()
                .Using(mergeSource1)
                .On((q, s) => q.QueueName == s.QueueName && q.MethodName == s.MethodName &&
                              q.TypeExecutedOn == s.TypeExecutedOn)
                .InsertWhenNotMatched();
            conn.GetTable<SqlCommonOddJobParamMetaData>().TableName(table.ParamTableName).Merge()
                .Using(mergeSource2)
                .On((q, s) => q.JobGuid == s.JobGuid && q.ParamOrdinal == s.ParamOrdinal)
                .InsertWhenNotMatched();
            conn.GetTable<SqlDbOddJobMethodGenericInfo>().TableName(table.JobMethodGenericParamTableName).Merge()
                .Using(mergeSource3)
                .On((q, s) => q.JobGuid == s.JobGuid && q.ParamOrder == s.ParamOrder)
                .InsertWhenNotMatched();
        }
        */

        public SqlCommonDbOddJobMetaData GetMetaDataForJob(SerializableOddJob jobData)
        {
            return new SqlCommonDbOddJobMetaData()
            {
                QueueName = jobData.QueueName,
                TypeExecutedOn = jobData.TypeExecutedOn,
                MethodName = jobData.MethodName,
                DoNotExecuteBefore = jobData.ExecutionTime,
                JobGuid = jobData.JobId,
                Status = JobStates.Inserting,
                CreatedDate = DateTime.Now,
                MaxRetries = (jobData.RetryParameters == null ? 0 : (int) jobData.RetryParameters.MaxRetries),
                MinRetryWait =
                    jobData.RetryParameters == null ? 0 : (int) jobData.RetryParameters.MinRetryWait.TotalSeconds,
                RetryCount = 0
            };
        }

        public SqlCommonOddJobParamMetaData[] GetParamDataForJob(SerializableOddJob jobData)
        {
            return jobData.JobArgs.Select(q => new SqlCommonOddJobParamMetaData()
            {
                JobGuid = jobData.JobId,
                ParamOrdinal = q.Ordinal,
                SerializedValue = q.Value,
                SerializedType = q.TypeName,
                ParameterName = q.Name
            }).ToArray();

        }

        public SqlDbOddJobMethodGenericInfo[] GetGenericDataForJob(SerializableOddJob jobData)
        {
            return jobData.MethodGenericTypes.Select((q, i) => new SqlDbOddJobMethodGenericInfo()
            {
                JobGuid = jobData.JobId,
                ParamOrder = i,
                ParamTypeName = q
            }).ToArray();

        }


        protected virtual BulkCopyOptions BulkOptions
        {
            get
            {
                return new BulkCopyOptions()
                {
                    KeepIdentity = false,
                };
            }
        }
        
        private async Task _addJobImplAsync(SerializableOddJob jobData, DataConnection conn, CancellationToken cancellationToken = default)
        {

            var table = _tableResolver.GetConfigurationForJob(jobData);
            //var jobMetaData = GetMetaDataForJob(jobData);
            var paramData = GetParamDataForJob(jobData);
            var jobGenParams = GetGenericDataForJob(jobData);
            var insertedIdExpr = conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(table.QueueTableName)
                .Value(q => q.QueueName, jobData.QueueName)
                .Value(q => q.TypeExecutedOn, jobData.TypeExecutedOn)
                .Value(q => q.MethodName, jobData.MethodName)
                .Value(q => q.DoNotExecuteBefore, jobData.ExecutionTime)
                .Value(q => q.JobGuid, jobData.JobId)
                .Value(q => q.Status, JobStates.Inserting)
                .Value(q => q.CreatedDate, DateTime.Now)
                .Value(q => q.MaxRetries, (jobData.RetryParameters == null ? 0 : (int?) jobData.RetryParameters.MaxRetries))
                .Value(q => q.MinRetryWait,
                    jobData.RetryParameters == null ? 0 : (int?) jobData.RetryParameters.MinRetryWait.TotalSeconds)
                .Value(q => q.RetryCount, 0);
            var insertedId = await insertedIdExpr.InsertWithInt64IdentityAsync(cancellationToken);

            if (paramData.Length > 1)
            {
                conn.GetTable<SqlCommonOddJobParamMetaData>().TableName(table.ParamTableName)
                    .BulkCopy(BulkOptions, paramData);
            }
            else
            {
                await conn.GetTable<SqlCommonOddJobParamMetaData>()
                    .TableName(table.ParamTableName)
                    .Value(r => r.JobGuid, paramData[0].JobGuid)
                    .Value(r => r.ParameterName, paramData[0].ParameterName)
                    .Value(r => r.ParamOrdinal, paramData[0].ParamOrdinal)
                    .Value(r => r.SerializedType, paramData[0].SerializedType)
                    .Value(r => r.SerializedValue, paramData[0].SerializedValue)
                    .InsertAsync(cancellationToken);
            }

            if (jobGenParams.Length > 0)
            {
                conn.GetTable<SqlDbOddJobMethodGenericInfo>().TableName(table.JobMethodGenericParamTableName)
                    .BulkCopy(BulkOptions,jobGenParams);
            }


            await conn.GetTable<SqlCommonDbOddJobMetaData>()
                .TableName(table.QueueTableName).Where(q => q.Id == insertedId)
                .Set(q => q.Status, JobStates.New)
                .UpdateAsync(cancellationToken);
        }

        private void _addJobImpl(SerializableOddJob jobData, DataConnection conn)
        {

            var table = _tableResolver.GetConfigurationForJob(jobData);
            //var jobMetaData = GetMetaDataForJob(jobData);
            var paramData = GetParamDataForJob(jobData);
            var jobGenParams = GetGenericDataForJob(jobData);
            var insertedIdExpr = conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(table.QueueTableName)
                .Value(q => q.QueueName, jobData.QueueName)
                .Value(q => q.TypeExecutedOn, jobData.TypeExecutedOn)
                .Value(q => q.MethodName, jobData.MethodName)
                .Value(q => q.DoNotExecuteBefore, jobData.ExecutionTime)
                .Value(q => q.JobGuid, jobData.JobId)
                .Value(q => q.Status, JobStates.Inserting)
                .Value(q => q.CreatedDate, DateTime.Now)
                .Value(q => q.MaxRetries, (jobData.RetryParameters == null ? 0 : (int?) jobData.RetryParameters.MaxRetries))
                .Value(q => q.MinRetryWait,
                    jobData.RetryParameters == null ? 0 : (int?) jobData.RetryParameters.MinRetryWait.TotalSeconds)
                .Value(q => q.RetryCount, 0);
            var insertedId = insertedIdExpr.InsertWithInt64Identity();

            if (paramData.Length > 0)
            {
                conn.GetTable<SqlCommonOddJobParamMetaData>().TableName(table.ParamTableName)
                    .BulkCopy(BulkOptions, paramData);
            }

            if (jobGenParams.Length > 0)
            {
                conn.GetTable<SqlDbOddJobMethodGenericInfo>().TableName(table.JobMethodGenericParamTableName)
                    .BulkCopy(BulkOptions,jobGenParams);
            }
            

            conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(table.QueueTableName).Where(q => q.Id == insertedId)
                .Set(q => q.Status, JobStates.New)
                .Update();
        }

        public virtual Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
                var ser = SerializableJobCreator.CreateJobDefinition(jobExpression, retryParameters, executionTime,queueName);
                AddJob(ser);
                return ser.JobId;
        }
        
        public virtual async Task<Guid> AddJobAsync<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default", CancellationToken cancellationToken = default)
        {
                var ser = SerializableJobCreator.CreateJobDefinition(jobExpression, retryParameters, executionTime,queueName);
                await AddJobAsync(ser, cancellationToken);
                return ser.JobId;
        }
        
        public virtual Guid AddJob<TJob>(Expression<Action<Guid,TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var ser = SerializableJobCreator.CreateJobDefinition(jobExpression, retryParameters, executionTime,queueName);
                AddJob(ser);
                return ser.JobId;
            }
        }
    }
}