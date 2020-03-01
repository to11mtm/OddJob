using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace GlutenFree.OddJob.Serializable
{
    public class UnversionedTypeSerializer : ITypeNameSerializer
    {
        public static ConcurrentDictionary<Type,string> scrubbedStringCache = new ConcurrentDictionary<Type, string>();
        public string GetTypeName(Type type)
        {
            if (type != null)
            {
                return scrubbedStringCache.GetOrAdd(type, typ =>
                {
                    var str =
                        _assemblyRegex.Replace(typ.AssemblyQualifiedName, "");
                    var str2 = str.Replace(
                        TargetPlatformHelpers.GetCoreAssemblyName(),
                        TargetPlatformHelpers.coreLibString);
                    return str2;
                });
            }

            return "";
        }


        //(\[[^,]*?,\s*[^,]*?)(,[^\]]*?)(\])(\],[^,]*)
        //(\[[^,]*?,\s*[^,]*?)(,[^\]]*?)(\])
        internal static Regex _assemblyRegex = new Regex(@"(\,\s*Version\=\S*\sCulture\S*,\sPublicKeyToken=[A-Za-z0-9_]*)", RegexOptions.Compiled);
    }
}