namespace GlutenFree.OddJob.Storage.Sql.Common
{
    public class SqlDbJobQueueDefaultTableConfiguration : ISqlDbJobQueueTableConfiguration
    {
        public const string DefaultQueueTableName = "QueueTable";
        public const string DefaultQueueParamTableName = "QueueParamValue";
        public const string DefaultJobMethodGenericParamTableName = "QueueJobMethodGenericParam";
        public string QueueTableName { get { return DefaultQueueTableName; } }
        public string ParamTableName { get { return DefaultQueueParamTableName; } }
        public int JobClaimLockTimeoutInSeconds { get { return 180; } }
        public string JobMethodGenericParamTableName
        {
            get { return DefaultJobMethodGenericParamTableName; }
        }
    }
}