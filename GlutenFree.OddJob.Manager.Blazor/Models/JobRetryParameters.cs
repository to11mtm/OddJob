namespace GlutenFree.OddJob.Manager.Blazor.Controllers;

public class JobRetryParameters
{
    public int MaxRetries;
    public TimeSpan MinRetryWait;
    public int RetryCount;
    public DateTime? LastAttempt;
}