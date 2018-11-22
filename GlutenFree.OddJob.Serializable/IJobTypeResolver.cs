using System;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public interface IJobTypeResolver
    {
        Type GetTypeForJob(string assemblyQualifiedTypeName);
    }
}