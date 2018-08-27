using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public static class SqlServerDbJobTableHelper
    {
        public static string JobTableCreateScript(ISqlDbJobQueueTableConfiguration configuration)
        {
            return string.Format(@"

create Table {0}
(
Id BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
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
JobParamId bigint not null identity(1,1) primary key,
JobGuid uniqueidentifier not null, 
ParamOrdinal int not null,
SerializedValue nvarchar(max) null,
SerializedType nvarchar(255) null
)", config.ParamTableName);
        }

        public static string JobQueueJobMethodGenericParamTableCreateScript(ISqlDbJobQueueTableConfiguration config)
        {
            return string.Format(@"
create table {0}
(
Id bigint not null identity(1,1) primary key,
JobGuid UniqueIdentifier not null,
ParamOrder int not null,
ParamTypeName nvarchar(255) not null,
)", config.JobMethodGenericParamTableName);
        }
    }
}
