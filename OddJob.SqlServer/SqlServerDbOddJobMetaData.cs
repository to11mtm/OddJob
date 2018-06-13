using System;
namespace OddJob.SqlServer
{
    public class SqlServerDbOddJobMetaData
    {
        public int JobId { get; set; }
        public Guid JobGuid { get; set; }
        public object[] JobArgs { get; set; }

        public string TypeExecutedOn { get; set; }

        public string MethodName { get; set; }
    }
}
