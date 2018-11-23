using System;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public class ThrowOnMissingJobTypeResolver : IJobTypeResolver
    {
        public Type GetTypeForJob(string assemblyQualifiedTypeName)
        {
            return Type.GetType(assemblyQualifiedTypeName);
        }
    }
}