using System;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IContainerFactory
    {
        object CreateInstance(Type typeToCreate);
    }
}