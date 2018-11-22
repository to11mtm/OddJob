using System;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public class NullOnMissingTypeJobTypeResolver : IJobTypeResolver
    {
        public Type GetTypeForJob(string assemblyQualifiedTypeName)
        {
            try
            {
                return  Type.GetType(assemblyQualifiedTypeName);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}