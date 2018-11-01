using System;

namespace GlutenFree.OddJob.Interfaces
{


    public interface IOddJob
    {
        Guid JobId { get; }
        OddJobParameter[] JobArgs { get; }
        Type TypeExecutedOn { get; }
        string MethodName { get; }
        string Status { get; }
        Type[] MethodGenericTypes { get; }
    }
}