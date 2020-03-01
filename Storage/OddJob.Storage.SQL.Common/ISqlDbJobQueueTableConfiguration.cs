namespace GlutenFree.OddJob.Storage.Sql.Common
{
    public interface ISqlDbJobQueueTableConfiguration
    {
        string QueueTableName { get; }
        string ParamTableName { get; }
        int JobClaimLockTimeoutInSeconds { get; }
        string JobMethodGenericParamTableName { get; }
    }
}