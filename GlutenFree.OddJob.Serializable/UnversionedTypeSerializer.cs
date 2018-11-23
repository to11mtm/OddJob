using System;
using System.Text.RegularExpressions;

namespace GlutenFree.OddJob.Serializable
{
    public class UnversionedTypeSerializer : ITypeNameSerializer
    {
        public string GetTypeName(Type type)
        {
            if (type != null)
            {
                var str = _assemblyRegex.Replace(type.AssemblyQualifiedName, "");
                return str;
            }

            return "";
        }
        //(\[[^,]*?,\s*[^,]*?)(,[^\]]*?)(\])(\],[^,]*)
        //(\[[^,]*?,\s*[^,]*?)(,[^\]]*?)(\])
        internal static Regex _assemblyRegex = new Regex(@"(\,\s*Version\=\S*\sCulture\S*,\sPublicKeyToken=[A-Za-z0-9_]*)", RegexOptions.Compiled);
    }
}