namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public class SqlDbJobQueueDefaultTableConfiguration : ISqlDbJobQueueTableConfiguration
    {
        public const string DefaultQueueTableName = "QueueTable";
        public const string DefaultQueueParamTableName = "QueueParamValue";
        public string QueueTableName { get { return DefaultQueueTableName; } }
        public string ParamTableName { get { return DefaultQueueParamTableName; } }
        public int JobClaimLockTimeoutInSeconds { get { return 180; } }
    }
}