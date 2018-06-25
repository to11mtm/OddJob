using System;

namespace OddJob.Storage.SqlServer.DbDtos
{
    public class SqlServerDbOddJobMetaData
    {
        public int JobId { get; set; }
        public Guid JobGuid { get; set; }
        public object[] JobArgs { get; set; }

        public string TypeExecutedOn { get; set; }

        public string MethodName { get; set; }

        public int MaxRetries { get; set; }
        public int MinRetryWait { get; set; }
        public DateTime? LastAttempt { get; set; }
        public int RetryCount { get; set; }
        public string Status { get; set; }
    }
}
