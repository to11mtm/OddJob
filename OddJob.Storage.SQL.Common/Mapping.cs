using System.Reflection.Emit;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    static class Mapping
    {
        public static MappingSchema BuildMappingSchema(ISqlDbJobQueueTableConfiguration _jobQueueTableConfiguration)
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
            return mapper.MappingSchema;
        }
    }
}