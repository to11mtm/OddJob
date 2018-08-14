using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
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
DoNotExecuteBefore text null,
JobGuid varchar(36) not null,
Status NVarChar(32) not null,
MaxRetries int,
MinRetryWait int,
RetryCount int,
LockClaimTime text null,
LockGuid VarChar(36) null,
LastAttempt text null,
CreatedDate text not null
)
", configuration.QueueTableName);
        }

        public static string JobQueueParamTableCreateScript(ISqlDbJobQueueTableConfiguration config)
        {
            return string.Format(@"
Create table {0}
(
JobParamId INTEGER PRIMARY KEY,
Id varchar(36) not null, 
ParamOrdinal int not null,
SerializedValue text null,
SerializedType nvarchar(255) null
)", config.ParamTableName);
        }
    }
}