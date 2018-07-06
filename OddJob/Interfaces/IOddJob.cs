using System;

namespace GlutenFree.OddJob.Interfaces
{


    public interface IOddJob
    {
        Guid JobId { get; }
        object[] JobArgs { get; }
        Type TypeExecutedOn { get; }
        string MethodName { get; }
        string Status { get; }
    }
}