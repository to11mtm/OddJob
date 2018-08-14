namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public interface ISqlServerJobQueueTableConfiguration
    {
        string QueueTableName { get; }
        string ParamTableName { get; }
        int JobClaimLockTimeoutInSeconds { get; }
    }
}