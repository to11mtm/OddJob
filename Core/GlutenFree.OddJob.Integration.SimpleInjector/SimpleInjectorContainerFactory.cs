using System;
using GlutenFree.OddJob.Interfaces;
using SimpleInjector;

namespace GlutenFree.OddJob.Integration.SimpleInjector
{
    public class SimpleInjectorContainerFactory : IContainerFactory
    {
        private Container _container;

        public SimpleInjectorContainerFactory(Container container)
        {
            _container = container;
        }
        public object CreateInstance(Type typeToCreate)
        {
            return _container.GetInstance(typeToCreate);
        }

        public void Release(object usedInstance)
        {
        }
    }
}