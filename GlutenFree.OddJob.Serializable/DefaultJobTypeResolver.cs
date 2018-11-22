using System;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public class DefaultJobTypeResolver : IJobTypeResolver
    {
        public Type GetTypeForJob(string assemblyQualifiedTypeName)
        {
            return Type.GetType(assemblyQualifiedTypeName);
        }
    }
}