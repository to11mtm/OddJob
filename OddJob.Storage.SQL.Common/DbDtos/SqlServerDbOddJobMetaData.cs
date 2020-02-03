﻿using System;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.Common.DbDtos
{
    public class SqlCommonDbOddJobMetaData
    {
        [Identity]
        [PrimaryKey]
        public long Id { get; set; }

        public Guid JobGuid { get; set; }

        [Column(CanBeNull = false)]
        public string TypeExecutedOn { get; set; }

        [Column(CanBeNull = false)]
        public string MethodName { get; set; }

        public int MaxRetries { get; set; }
        public int MinRetryWait { get; set; }
        public DateTime? LastAttempt { get; set; }
        public int RetryCount { get; set; }
        [Column(CanBeNull = false, Scale = 20)]
        public string Status { get; set; }
        public string QueueName { get; set; }
        public DateTimeOffset? DoNotExecuteBefore { get; set; }
        public DateTime? LockClaimTime { get; set; }
        public Guid? LockGuid { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
