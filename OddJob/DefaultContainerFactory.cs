using System;

namespace OddJob
{
    public class DefaultContainerFactory : IContainerFactory
    {
        public object CreateInstance(Type typeToCreate)
        {
            return Activator.CreateInstance(typeToCreate);
        }
    }
}