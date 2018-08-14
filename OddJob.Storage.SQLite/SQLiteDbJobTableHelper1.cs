using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.Sql.SQLite
{
    public static class SQLiteDbJobTableHelper
    {
        public static string JobTableCreateScript(SqlDbJobQueueDefaultTableConfiguration configuration)
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

        public static string JobQueueParamTableCreateScript(SqlDbJobQueueDefaultTableConfiguration config)
        {
            return string.Format(@"
Create table {0}
(
JobParamId INTEGER PRIMARY KEY,
Id uniqueidentifier not null, 
ParamOrdinal int not null,
SerializedValue text null,
SerializedType nvarchar(255) null
)", config.ParamTableName);
        }
    }
}
