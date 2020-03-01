using System;

namespace GlutenFree.OddJob.Serializable
{
    public class NullOnMissingTypeJobTypeResolver : IJobTypeResolver
    {
        public Type GetTypeForJob(string assemblyQualifiedTypeName)
        {
            try
            {
                return  Type.GetType(TargetPlatformHelpers.ReplaceCoreTypes(assemblyQualifiedTypeName));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}