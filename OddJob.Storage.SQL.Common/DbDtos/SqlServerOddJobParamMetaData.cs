using System;

namespace GlutenFree.OddJob.Storage.SQL.Common.DbDtos
{
    public class SqlCommonOddJobParamMetaData
    {
        public Guid JobGuid { get; set; }
        public int ParamOrdinal { get; set; }
        public string SerializedValue { get; set; }
        public string SerializedType { get; set; }
    }
}
