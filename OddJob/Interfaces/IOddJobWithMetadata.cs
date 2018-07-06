namespace GlutenFree.OddJob.Interfaces
{
    public interface IOddJobWithMetadata : IOddJob
    {
        IRetryParameters RetryParameters { get; }
    }
}