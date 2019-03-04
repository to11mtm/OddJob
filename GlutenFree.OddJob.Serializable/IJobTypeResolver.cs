using System;

namespace GlutenFree.OddJob.Serializable
{
    public interface IJobTypeResolver
    {
        Type GetTypeForJob(string assemblyQualifiedTypeName);
    }
}