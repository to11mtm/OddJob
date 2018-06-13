using System;

namespace OddJob
{
    public interface IContainerFactory
    {
        object CreateInstance(Type typeToCreate);
    }
}