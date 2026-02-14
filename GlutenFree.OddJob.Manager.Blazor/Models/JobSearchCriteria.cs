namespace GlutenFree.OddJob.Manager.Blazor.Controllers;

public class JobSearchCriteria
{
    public bool UseMethod;
    public bool UseStatus;
    public bool useCreatedDate;
    public bool useLastAttemptDate;
    public DateTime? createdBefore = null;
    public DateTime? createdAfter = null;
    public DateTime? attemptedBeforeDate = null;
    public DateTime? attemptedAfterDate = null;
    public TimeOnly? attemptedBeforeTime = null;
    public TimeOnly? attemptedAfterTime = null;
    public TimeOnly? createdBeforeTime= null;
    public TimeOnly? createdAfterTime = null;
    public bool UseQueue = true;

    public string QueueName { get; set; }
    public string MethodName { get; set; }
    public string Status { get; set; }
    public Guid? JobGuid { get; set; }
}