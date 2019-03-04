namespace GlutenFree.OddJob.Serializable
{
    public static class TargetPlatformHelpers
    {
        private static object _coreAssemblyNameWrteLock = new object();
        private static string _assemblyCoreType = null;
        public const string coreLibString = "%coretarget%";

        public static string ReplaceCoreTypes(string typeString)
        {
            return typeString.Replace(coreLibString, GetCoreAssemblyName());
        }

        public static string GetCoreAssemblyName()
        {
            if (_assemblyCoreType == null)
            {
                lock (_coreAssemblyNameWrteLock)
                {
                    if (_assemblyCoreType == null)
                    {
                        _assemblyCoreType =
                            UnversionedTypeSerializer._assemblyRegex.Replace(typeof(string).Assembly.FullName, "");
                    }
                }
            }

            return _assemblyCoreType;
        }
    }
}