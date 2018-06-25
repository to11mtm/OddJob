namespace OddJob.Storage.SqlServer
{
    public interface ISqlServerJobQueueTableConfiguration
    {
        string QueueTableName { get; }
        string ParamTableName { get; }
        int JobClaimLockTimeoutInSeconds { get; }
    }
}