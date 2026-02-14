using MagicOnion.Server;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace OddJob.Rpc.Integration.SimpleInjector
{
    public class SimpleInjectorServiceLocator : IServiceLocator
    {
        public SimpleInjectorServiceLocator(Container container)
        {
            _container = container;
        }
        private Container _container;
        public T GetService<T>() 
        {
            return (T)_container.GetInstance(typeof(T));
        }

        public IServiceLocatorScope CreateScope()
        {
            return new SimpleInjectorServiceLocatorScope(AsyncScopedLifestyle.BeginScope(_container));
        }
    }
}