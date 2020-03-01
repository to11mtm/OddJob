using System;
using MagicOnion.Server;

namespace OddJob.Rpc.Integration.SimpleInjector
{
    public class SimpleInjectorActivator : IMagicOnionServiceActivator
    {
        public T Create<T>(IServiceLocator serviceLocator)
        {
            return serviceLocator.GetService<T>();
        }
    }
}