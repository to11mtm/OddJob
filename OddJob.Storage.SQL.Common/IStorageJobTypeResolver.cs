using System;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public interface IStorageJobTypeResolver
    {
        Type GetTypeForJob(string assemblyQualifiedTypeName);
    }
}