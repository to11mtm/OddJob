using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public class DefaultContainerFactory : IContainerFactory
    {
        public object CreateInstance(Type typeToCreate)
        {
            return Activator.CreateInstance(typeToCreate);
        }

        public void Release(object usedInstance)
        {
        }
    }
}