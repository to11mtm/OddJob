using System;

namespace GlutenFree.OddJob.Serializable
{
    public class VersionedTypeSerializer : ITypeNameSerializer
    {
        public string GetTypeName(Type type)
        {
            if (type != null)
            {
                return type.AssemblyQualifiedName;
            }

            return "";

        }
    }
}