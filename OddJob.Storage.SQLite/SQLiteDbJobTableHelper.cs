using OddJob.Storage.SQL.Common;

namespace OddJob.Storage.SQLite
{
    public static class SQLiteDbJobTableHelper
    {
        public static string JobTableCreateScript(ISqlServerJobQueueTableConfiguration configuration)
        {
            return string.Format(@"

create Table {0}
(
JobId INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
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

        public static string JobQueueParamTableCreateScript(ISqlServerJobQueueTableConfiguration config)
        {
            return string.Format(@"
Create table {0}
(
JobParamId int not null identity(1,1) primary key,
JobId uniqueidentifier not null, 
ParamOrdinal int not null,
SerializedValue nvarchar(max) null,
SerializedType nvarchar(255) null
)", config.ParamTableName);
        }
    }
}