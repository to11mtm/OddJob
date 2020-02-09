using System;
using GlutenFree.OddJob.Interfaces;
using SimpleInjector;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class TestSimpleInjectorContainerFactory : IContainerFactory
    {
        private Container _container;

        public TestSimpleInjectorContainerFactory(Container container)
        {
            _container = container;
        }
        public object CreateInstance(Type typeToCreate)
        {
            return _container.GetInstance(typeToCreate);
        }

        public void Relase(object usedInstance)
        {
        }
    }
}