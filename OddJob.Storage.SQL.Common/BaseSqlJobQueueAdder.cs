using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public abstract class BaseSqlJobQueueAdder : IJobQueueAdder, ISerializedJobQueueAdder
    {
        private readonly FluentMappingBuilder _mappingSchema;
        private readonly IJobAdderQueueTableResolver _tableResolver;
        protected BaseSqlJobQueueAdder(IJobQueueDataConnectionFactory jobQueueDataConnectionFactory, IJobAdderQueueTableResolver tableResolver)
        {
            _jobQueueConnectionFactory = jobQueueDataConnectionFactory;

            _mappingSchema = MappingSchema.Default.GetFluentMappingBuilder();
            _tableResolver = tableResolver;
        }

        
        
        private IJobQueueDataConnectionFactory _jobQueueConnectionFactory { get; set; }


        public virtual void AddJobs(IEnumerable<SerializableOddJob> jobDatas)
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                foreach (var job in jobDatas)
                {
                    _addJobImpl(job,conn);
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

        private void _addJobImpl(SerializableOddJob jobData, DataConnection conn)
        {
            var table = _tableResolver.GetConfigurationForJob(jobData);
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
                    jobData.RetryParameters == null ? 0 : (double?) jobData.RetryParameters.MinRetryWait.TotalSeconds)
                .Value(q => q.RetryCount, 0);
            var insertedId = insertedIdExpr.InsertWithInt64Identity();


            var toInsert = jobData.JobArgs.Select((val, index) => new {val, index}).ToList();
            toInsert.ForEach(i =>
            {
                conn.GetTable<SqlCommonOddJobParamMetaData>().TableName(table.ParamTableName)
                    .Value(q => q.JobGuid, jobData.JobId)
                    .Value(q => q.ParamOrdinal, i.index)
                    .Value(q => q.SerializedValue, i.val.Value)
                    .Value(q => q.SerializedType, i.val.TypeName)
                    .Value(q => q.ParameterName, i.val.Name)
                    .Insert();
            });

            var genMethodArgs = jobData.MethodGenericTypes.Select((val, index) => new {val, index}).ToList();
            genMethodArgs.ForEach(i =>
            {
                conn.GetTable<SqlDbOddJobMethodGenericInfo>().TableName(table.JobMethodGenericParamTableName)
                    .Value(q => q.JobGuid, jobData.JobId)
                    .Value(q => q.ParamOrder, i.index)
                    .Value(q => q.ParamTypeName, i.val)
                    .Insert();
            });

            conn.GetTable<SqlCommonDbOddJobMetaData>().TableName(table.QueueTableName).Where(q => q.Id == insertedId)
                .Set(q => q.Status, JobStates.New)
                .Update();
        }

        public virtual Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
            using (var conn = _jobQueueConnectionFactory.CreateDataConnection(_mappingSchema.MappingSchema))
            {
                var ser = SerializableJobCreator.CreateJobDefiniton(jobExpression, retryParameters, executionTime,queueName);
                AddJob(ser);
                return ser.JobId;
            }
        }
    }
}