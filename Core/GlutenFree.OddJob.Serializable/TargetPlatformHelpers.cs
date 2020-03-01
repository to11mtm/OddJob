using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GlutenFree.OddJob.Serializable
{
    public static class TargetPlatformHelpers
    {
        private static object _coreAssemblyNameWrteLock = new object();
        private static string _assemblyCoreType = null;
        public const string coreLibString = "%coretarget%";
        private static ConcurrentDictionary<string,string> _typeNameCache = new ConcurrentDictionary<string, string>();
        public static string ReplaceCoreTypes(string typeString)
        {
            if (_typeNameCache.ContainsKey(typeString) == false)
            {
                return _typeNameCache.GetOrAdd(typeString,
                    ts => typeString.Replace(coreLibString,
                        GetCoreAssemblyName()));
            }
            return _typeNameCache[typeString];
            
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