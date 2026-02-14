using System;

namespace GlutenFree.OddJob.Serializable
{
    public class ThrowOnMissingJobTypeResolver : IJobTypeResolver
    {
        public Type GetTypeForJob(string assemblyQualifiedTypeName)
        {
            return Type.GetType(TargetPlatformHelpers.ReplaceCoreTypes(assemblyQualifiedTypeName));
        }
    }
}