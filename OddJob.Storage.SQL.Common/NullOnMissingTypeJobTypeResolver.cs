using System;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public class NullOnMissingTypeJobTypeResolver : IStorageJobTypeResolver
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