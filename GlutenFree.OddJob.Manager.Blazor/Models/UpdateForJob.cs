namespace GlutenFree.OddJob.Manager.Blazor.Controllers;

public class UpdateForJob
{
    public Guid JobGuid;
    public string OldStatus;
    public bool UpdateRetryCount;
    public int NewMaxRetryCount;
        
    public bool RequireOldStatus;
    public bool UpdateMethodName;
    public string NewMethodName;
    public bool UpdateQueueName;
    public string NewQueueName;
    public bool UpdateStatus;
    public string NewStatus;
    public UpdateForParam[] ParamUpdates = new UpdateForParam[]{};
}