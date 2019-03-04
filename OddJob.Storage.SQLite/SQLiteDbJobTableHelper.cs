using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.Sql.SQLite
{
    public static class SQLiteDbJobTableHelper
    {
        public static string JobTableCreateScript(ISqlDbJobQueueTableConfiguration configuration)
        {
            return string.Format(@"

create Table {0}
(
Id INTEGER PRIMARY KEY,
QueueName NVarchar(255) not null,
TypeExecutedOn NVarChar(255) not null,
MethodName NVarChar(255) not null,
DoNotExecuteBefore datetime null,
JobGuid uniqueidentifier not null,
Status NVarChar(32) not null,
MaxRetries int,
MinRetryWait int,
RetryCount int,
LockClaimTime datetime null,
LockGuid uniqueidentifier null,
LastAttempt datetime null,
CreatedDate datetime not null)
", configuration.QueueTableName);
        }

        public static string JobQueueParamTableCreateScript(ISqlDbJobQueueTableConfiguration config)
        {
            return string.Format(@"
Create table {0}
(
Id INTEGER PRIMARY KEY,
JobGuid uniqueidentifier not null, 
ParamOrdinal int not null,
SerializedValue text null,
SerializedType nvarchar(255) null,
ParameterName nvarchar(255) null
)", config.ParamTableName);
        }

        public static string JobQueueJobMethodGenericParamTableCreateScript(ISqlDbJobQueueTableConfiguration config)
        {
            return string.Format(@"
create table {0}
(
Id INTEGER primary key,
JobGuid UniqueIdentifier not null,
ParamOrder int not null,
ParamTypeName text not null
)", config.JobMethodGenericParamTableName);
        }

        public static string SuggestedIndexes(ISqlDbJobQueueTableConfiguration config)
        {
            return string.Format(@"
create index {2}_jobguid_idx on {2}(JobGuid);
create index {1}_jobguid_idx on {1}(JobGuid);
create unique  index {0}_jobguid_idx on {0}(JobGuid);
", config.QueueTableName, config.ParamTableName, config.JobMethodGenericParamTableName);
        }
    }
}
