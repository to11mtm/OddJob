using System;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.Common.DbDtos
{
    public class SqlCommonOddJobParamMetaData
    {
        [Identity]
        [PrimaryKey]
        public long Id { get; set; }
        public Guid JobGuid { get; set; }
        public int ParamOrdinal { get; set; }
        [NotNull]
        public string SerializedValue { get; set; }
        [NotNull]
        public string SerializedType { get; set; }
        [NotNull]
        public string ParameterName { get; set; }
    }
}
