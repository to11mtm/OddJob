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
        [Column(CanBeNull = false, Length = Int32.MaxValue)]
        public string SerializedValue { get; set; }
        [Column(CanBeNull = false,Length = 1024)]
        public string SerializedType { get; set; }
        [Column(CanBeNull = false, Length =256)]
        public string ParameterName { get; set; }
    }
}
