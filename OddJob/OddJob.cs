using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public class OddJob : IOddJob
    {
        public Guid JobId { get; protected set; }
        public object[] JobArgs { get; protected set; }
        public Type TypeExecutedOn { get; protected set; }
        public string MethodName { get; protected set; }
        public string Status { get; protected set; }
        public Type[] MethodGenericTypes { get; protected set; }
        public OddJob(string methodName, object[] jobArgs, Type typeExecutedOn, Type[] methodGenericTypes, string status=JobStates.New)
        {
            JobId = Guid.NewGuid();
            MethodName = methodName;
            JobArgs = jobArgs;
            TypeExecutedOn = typeExecutedOn;
            Status = status;
            MethodGenericTypes = methodGenericTypes;
        }
    }
}