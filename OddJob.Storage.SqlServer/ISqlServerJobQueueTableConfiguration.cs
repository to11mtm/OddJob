namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public interface ISqlServerJobQueueTableConfiguration
    {
        string QueueTableName { get; }
        string ParamTableName { get; }
        int JobClaimLockTimeoutInSeconds { get; }
    }
}