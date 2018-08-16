namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public interface ISqlDbJobQueueTableConfiguration
    {
        string QueueTableName { get; }
        string ParamTableName { get; }
        int JobClaimLockTimeoutInSeconds { get; }
    }
}