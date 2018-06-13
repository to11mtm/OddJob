using System;
namespace OddJob.SqlServer
{
    public class SqlServerDbOddJob : IOddJob
    {
        public Guid JobId { get; set; }
        public object[] JobArgs { get; set; }

        public Type TypeExecutedOn { get; set; }

        public string MethodName { get; set; }
    }
}
