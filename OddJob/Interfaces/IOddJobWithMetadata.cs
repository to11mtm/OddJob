namespace OddJob
{
    public interface IOddJobWithMetadata : IOddJob
    {
        IRetryParameters RetryParameters { get; }
    }
}