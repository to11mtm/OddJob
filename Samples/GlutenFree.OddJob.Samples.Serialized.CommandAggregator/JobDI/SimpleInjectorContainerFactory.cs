using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    public class SimpleInjectorContainerFactory : IContainerFactory
    {
        private SimpleInjector.Container _container;

        public SimpleInjectorContainerFactory(SimpleInjector.Container container)
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