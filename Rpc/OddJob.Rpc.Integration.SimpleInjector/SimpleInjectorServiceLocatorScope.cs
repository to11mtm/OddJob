using MagicOnion.Server;
using SimpleInjector;

namespace OddJob.Rpc.Integration.SimpleInjector
{
    public class SimpleInjectorServiceLocatorScope : IServiceLocatorScope
    {
        private Scope _scope;
        public SimpleInjectorServiceLocatorScope(Scope beginScope)
        {
            _scope = beginScope;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public IServiceLocator ServiceLocator { get{return new SimpleInjectorServiceLocator(_scope.Container);} }
    }
}