using System;

namespace GlutenFree.OddJob.Serializable
{
    public interface ITypeNameSerializer
    {
        string GetTypeName(Type type);
    }
}