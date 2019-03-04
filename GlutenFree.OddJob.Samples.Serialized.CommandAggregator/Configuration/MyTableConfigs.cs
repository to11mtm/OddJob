using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    public class MyTableConfigs : ISqlDbJobQueueTableConfiguration
    {
        public string QueueTableName { get; set;}
        public string ParamTableName { get; set;}
        public int JobClaimLockTimeoutInSeconds { get; set;}
        public string JobMethodGenericParamTableName { get; set;}
    }
}