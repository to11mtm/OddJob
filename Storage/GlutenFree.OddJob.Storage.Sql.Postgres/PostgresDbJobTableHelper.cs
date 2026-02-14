using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.Postgres
{
    public static class PostgresDbJobTableHelper
    {
        public static string JobTableCreateScript(ISqlDbJobQueueTableConfiguration configuration)
        {
            return string.Format(@"
create table {0}
(
    Id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    QueueName VARCHAR(255) not null,
    TypeExecutedOn VARCHAR(255) not null,
    MethodName VARCHAR(255) not null,
    DoNotExecuteBefore TIMESTAMP null,
    JobGuid UUID not null,
    Status VARCHAR(32) not null,
    MaxRetries int,
    MinRetryWait int,
    RetryCount int,
    LockClaimTime TIMESTAMP null,
    LockGuid UUID null,
    LastAttempt TIMESTAMP null,
    CreatedDate TIMESTAMP not null
)", configuration.QueueTableName);
        }

        public static string JobQueueParamTableCreateScript(ISqlDbJobQueueTableConfiguration config)
        {
            return string.Format(@"
create table {0}
(
    Id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    JobGuid UUID not null,
    ParamOrdinal int not null,
    SerializedValue text null,
    SerializedType VARCHAR(255) null,
    ParameterName VARCHAR(255) null,
    MethodArgType VARCHAR(255) null
)", config.ParamTableName);
        }

        public static string JobQueueJobMethodGenericParamTableCreateScript(ISqlDbJobQueueTableConfiguration config)
        {
            return string.Format(@"
create table {0}
(
    Id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    JobGuid UUID not null,
    ParamOrdinal int not null,
    SerializedValue text null,
    SerializedType VARCHAR(255) null,
    ParameterName VARCHAR(255) null
)", config.JobMethodGenericParamTableName);
        }
    }
}
