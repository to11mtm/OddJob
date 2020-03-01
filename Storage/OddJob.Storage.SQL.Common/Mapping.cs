﻿using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    static class Mapping
    {
        public static FluentMappingBuilder BuildMappingSchema(ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration)
        {
            var mapper = new LinqToDB.Mapping.FluentMappingBuilder(MappingSchema.Default);
            mapper.Entity<SqlCommonDbOddJobMetaData>().HasAttribute(
                new TableAttribute(_jobQueueTableConfiguration.QueueTableName) { IsColumnAttributeRequired = false, Name = _jobQueueTableConfiguration.QueueTableName});
            mapper.Entity<SqlCommonOddJobParamMetaData>().HasAttribute(
                new TableAttribute(_jobQueueTableConfiguration.ParamTableName){IsColumnAttributeRequired = false, Name = _jobQueueTableConfiguration.ParamTableName});
            mapper.Entity<SqlDbOddJobMethodGenericInfo>().HasAttribute(
                new TableAttribute(_jobQueueTableConfiguration.JobMethodGenericParamTableName)
                {
                    IsColumnAttributeRequired = false,
                    Name = _jobQueueTableConfiguration.JobMethodGenericParamTableName
                });
            
            return mapper;
        }
    }
}