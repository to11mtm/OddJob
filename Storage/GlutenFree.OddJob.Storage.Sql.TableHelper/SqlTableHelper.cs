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
            ISqlDbJobQueueTableConfiguration tableConf, bool clusteredJobGuid = false)
        {
            List<string> idxRet = new List<string>();
            if (clusteredJobGuid)
            {
                idxRet.Add(DropPKConstraintImpl<SqlCommonDbOddJobMetaData>(tableConf.QueueTableName, "PK_" + tableConf.QueueTableName+"_ID"));
                //idxRet.Add(DropPKIndexImpl<SqlCommonDbOddJobMetaData>(tableConf.QueueTableName, "PK_" + tableConf.QueueTableName+"_ID"));
            }
            idxRet.Add(GenerateIndexImpl(
                Index<SqlCommonDbOddJobMetaData>.Create()
                    .Column(r => r.JobGuid),
                tableConf.QueueTableName,
                "IDX_" + tableConf.QueueTableName + "_JOBGUID", true,
                clusteredJobGuid));
            if (clusteredJobGuid)
            {
                
                idxRet.Add(GeneratePrimaryKeyImpl(Index<SqlCommonDbOddJobMetaData>.Create()
                    .Column(r=>r.Id),
                    tableConf.QueueTableName, "PK_" + tableConf.QueueTableName + "_ID"));
            }

            idxRet.Add(
                GenerateIndexImpl(Index<SqlCommonDbOddJobMetaData>.Create()
                        .Column(r => r.CreatedDate, DirectionEnum.asc),
                    tableConf.QueueTableName,
                    "IDX_" + tableConf.QueueTableName + "_CREATEDATE", false));
            
            
            idxRet.Add(
                GenerateIndexImpl(Index<SqlCommonDbOddJobMetaData>.Create()
                        .Column(r => r.QueueName)
                        .Column(r => r.Status), tableConf.QueueTableName,
                    "IDX_" + tableConf.QueueTableName + "_QN_STAT", false));
            
            idxRet.Add(
                GenerateIndexImpl(Index<SqlCommonDbOddJobMetaData>.Create()
                        .Column(r => r.LockGuid, DirectionEnum.unspecified),
                    tableConf.QueueTableName,
                    "IDX_" + tableConf.QueueTableName + "_LockGuid", false));
            return idxRet;
        }

        

        public List<string> CreateParamIndexes(
            ISqlDbJobQueueTableConfiguration tableConf, bool clusteredJobGuid= false)
        {
            List<string> idxRet = new List<string>();
            if (clusteredJobGuid)
            {
                idxRet.Add(DropPKConstraintImpl<SqlCommonOddJobParamMetaData>(tableConf.ParamTableName, "PK_" + tableConf.ParamTableName+"_ID"));
            }
            idxRet.Add(
             GenerateIndexImpl(
                Index<SqlCommonOddJobParamMetaData>.Create()
                    .Column(r => r.JobGuid), tableConf.ParamTableName,
                "IDX_" + tableConf.ParamTableName + "_JOBGUID", false, clusteredJobGuid));
            if (clusteredJobGuid)
            {
             
                idxRet.Add(GeneratePrimaryKeyImpl(Index<SqlCommonOddJobParamMetaData>.Create()
                        .Column(r=>r.Id),
                    tableConf.ParamTableName, "PK_" + tableConf.ParamTableName + "_ID"));
            }

            return idxRet;
        }

        public List<string> CreateQueueParamIndexes(
            ISqlDbJobQueueTableConfiguration tableConf, bool clusteredJobGuid = false)
        {
            List<string> idxRet = new List<string>();
            if (clusteredJobGuid)
            {
                idxRet.Add(DropPKConstraintImpl<SqlDbOddJobMethodGenericInfo>(tableConf.JobMethodGenericParamTableName, "PK_" + tableConf.JobMethodGenericParamTableName+"_ID"));
            }
            idxRet.Add(
                GenerateIndexImpl(
                    Index<SqlDbOddJobMethodGenericInfo>.Create()
                        .Column(r => r.JobGuid), tableConf.JobMethodGenericParamTableName,
                    "IDX_" + tableConf.JobMethodGenericParamTableName + "_JOBGUID", false, clusteredJobGuid));
            if (clusteredJobGuid)
            {
             
                idxRet.Add(GeneratePrimaryKeyImpl(Index<SqlDbOddJobMethodGenericInfo>.Create()
                        .Column(r=>r.Id),
                    tableConf.JobMethodGenericParamTableName, "PK_" + tableConf.JobMethodGenericParamTableName + "_ID"));
            }

            return idxRet;
        }

        private string GenerateIndexImpl<T>(Index<T> indexDef, string tableName, string indexName, bool unique, bool clustered = false)
        {
            using (var conn = _connFact.CreateDataConnection(MappingSchema.Default))
            {
                var helper = new Linq2DbGenerator(_generator, conn);
                return helper.IndexFor<T>(indexDef, indexName, unique, tableName, clustered);
            }
        }

        private string GeneratePrimaryKeyImpl<T>(Index<T> indexDef,
            string tableName, string constraintName)
        {
            using (var conn = _connFact.CreateDataConnection(MappingSchema.Default))
            {
                var helper = new Linq2DbGenerator(_generator, conn);
                return helper.PrimaryKeyFor(indexDef, constraintName,
                    tableName);
            }
        }
        private string DropPKConstraintImpl<T>(string queueTableName,string constraintName)
        {
            using (var conn =
                _connFact.CreateDataConnection(MappingSchema.Default))
            {
                var helper = new Linq2DbGenerator(_generator,conn);
                return helper.DropPrimaryKey<T>(constraintName, queueTableName);
            }
        }
        private string DropPKIndexImpl<T>(string tableName, string indexName)
        {
            using (var conn =
                _connFact.CreateDataConnection(MappingSchema.Default))
            {
                var helper = new Linq2DbGenerator(_generator,conn);
                return helper.DropIndexFor<T>(null, indexName, tableName);
            }
        }
        public string GetMainTableSql(ISqlDbJobQueueTableConfiguration tableConf)
        {
            return GenerateImpl<SqlCommonDbOddJobMetaData>(tableConf.QueueTableName, false);
        }

        private string GenerateImpl<T>(string tableName, bool nonclusteredPrimaryKey = false)
        {
            using (var conn = _connFact.CreateDataConnection(MappingSchema.Default))
            {
                var helper = new Linq2DbGenerator(_generator, conn);
                return helper.CreateTableFor<T>(tableName, nonclusteredPrimaryKey);
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
