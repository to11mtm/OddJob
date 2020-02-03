using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using FluentMigrator;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using GlutenFree.Linq2Db.FluentMigrator.Helpers;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using LinqToDB;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.TableHelper
{
    public class SqlTableHelper
    {
        public SqlTableHelper(IJobQueueDataConnectionFactory connFact, GenericGenerator generator)
        {
            _connFact = connFact;
            _generator = generator;
        }
        private IJobQueueDataConnectionFactory _connFact;
        private GenericGenerator _generator;

        public List<string> GetCreateMainTableIndexes(
            ISqlDbJobQueueTableConfiguration tableConf)
        {
            var mainIdx = GenerateIndexImpl(
                Index<SqlCommonDbOddJobMetaData>.Create()
                    .Column(r => r.JobGuid),
                tableConf.QueueTableName,
                "IDX_" + tableConf.QueueTableName + "_JOBGUID", true);

            var mainIdx2 =
                GenerateIndexImpl(Index<SqlCommonDbOddJobMetaData>.Create()
                        .Column(r => r.CreatedDate, DirectionEnum.asc),
                    tableConf.QueueTableName,
                    "IDX_" + tableConf.QueueTableName + "_CREATEDATE", false);
            var mainIdx3 =
                GenerateIndexImpl(Index<SqlCommonDbOddJobMetaData>.Create()
                        .Column(r => r.QueueName)
                        .Column(r => r.Status), tableConf.QueueTableName,
                    "IDX_" + tableConf.QueueTableName + "_QN_STAT", false);
            return new List<string>()
            {
                mainIdx,
                mainIdx2,
                mainIdx3
            };
        }

        public string CreateParamIndexes(
            ISqlDbJobQueueTableConfiguration tableConf)
        {
            return GenerateIndexImpl(
                Index<SqlCommonOddJobParamMetaData>.Create()
                    .Column(r => r.JobGuid), tableConf.ParamTableName,
                "IDX_" + tableConf.ParamTableName + "_JOBGUID", false);
        }

        public string CreateQueueParamIndexes(
            ISqlDbJobQueueTableConfiguration tableConf)
        {
            return GenerateIndexImpl(
                Index<SqlDbOddJobMethodGenericInfo>.Create()
                    .Column(r => r.JobGuid), tableConf.ParamTableName,
                "IDX_" + tableConf.JobMethodGenericParamTableName + "_JOBGUID",
                false);
        }

        private string GenerateIndexImpl<T>(Index<T> indexDef, string tableName, string indexName, bool unique)
        {
            using (var conn = _connFact.CreateDataConnection(MappingSchema.Default))
            {
                var helper = new Linq2DbGenerator(_generator, conn);
                return helper.IndexFor<T>(indexDef, indexName, unique, tableName);
            }
        }
        public string GetMainTableSql(ISqlDbJobQueueTableConfiguration tableConf)
        {
            return GenerateImpl<SqlCommonDbOddJobMetaData>(tableConf.QueueTableName);
        }

        private string GenerateImpl<T>(string tableName)
        {
            using (var conn = _connFact.CreateDataConnection(MappingSchema.Default))
            {
                var helper = new Linq2DbGenerator(_generator, conn);
                return helper.CreateTableFor<T>(tableName);
            }
        }
        public string GetParamTableSql(ISqlDbJobQueueTableConfiguration tableConf)
        {
            return GenerateImpl<SqlCommonOddJobParamMetaData>(tableConf.ParamTableName);
        }

        public string GetGenericParamTableSql(ISqlDbJobQueueTableConfiguration tableConf)
        {
            return GenerateImpl<SqlDbOddJobMethodGenericInfo>(tableConf.JobMethodGenericParamTableName);
        }

    }
}
