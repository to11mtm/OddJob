using System;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public class DefaultStorageJobTypeResolver : IStorageJobTypeResolver
    {
        public Type GetTypeForJob(string assemblyQualifiedTypeName)
        {
            return Type.GetType(assemblyQualifiedTypeName);
        }
    }
}