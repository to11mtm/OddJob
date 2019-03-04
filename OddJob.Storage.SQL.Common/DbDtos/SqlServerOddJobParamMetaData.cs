using System;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common.DbDtos
{
    public class SqlCommonOddJobParamMetaData
    {
        [Identity]
        [PrimaryKey]
        public long Id { get; set; }
        public Guid JobGuid { get; set; }
        public int ParamOrdinal { get; set; }
        public string SerializedValue { get; set; }
        public string SerializedType { get; set; }
        public string ParameterName { get; set; }
    }
}
