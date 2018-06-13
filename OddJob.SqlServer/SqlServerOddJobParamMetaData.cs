namespace OddJob.SqlServer
{
    public class SqlServerOddJobParamMetaData
    {
        public int JobId { get; set; }
        public int ParamOrdinal { get; set; }
        public string SerializedValue { get; set; }
        public string SerializedType { get; set; }
    }
}
