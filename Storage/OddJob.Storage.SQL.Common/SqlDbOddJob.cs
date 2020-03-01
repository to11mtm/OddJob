using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Storage.Sql.Common
{

    public class SqlDbOddJob : IOddJobWithMetadata
    {
        public Guid JobId { get; set; }
        public OddJobParameter[] JobArgs { get; set; }

        public Type TypeExecutedOn { get; set; }

        public string MethodName { get; set; }
        public string Status { get; set; }
        public Type[] MethodGenericTypes { get; set; }
        public IRetryParameters RetryParameters { get; set; }
        public string Queue { get; set; }
        public DateTimeOffset? ExecutionTime { get; set; }
    }
}
