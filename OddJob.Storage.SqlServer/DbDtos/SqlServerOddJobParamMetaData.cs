using System;

namespace OddJob.Storage.SqlServer.DbDtos
{
    public class SqlServerOddJobParamMetaData
    {
        public Guid JobId { get; set; }
        public int ParamOrdinal { get; set; }
        public string SerializedValue { get; set; }
        public string SerializedType { get; set; }
    }
}
