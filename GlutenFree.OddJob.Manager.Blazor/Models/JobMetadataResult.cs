namespace GlutenFree.OddJob.Manager.Blazor.Controllers;

public class JobMetadataResult
{
    public Guid JobId;
    public JobParameterDto[] JobArgs;
    public string TypeExecutedOn;
    public string MethodName;
    public string Status;
    public string[] MethodGenericTypes;
    public string ExecutionTime;
    public JobRetryParameters RetryParameters;
    public string Queue;
}