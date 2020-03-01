using System;
using System.Threading.Tasks;
using SimpleInjector;
using SimpleInjector.Lifestyles;

[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
{
    //We use this to help get around Websharper's treatment of all Remoting handlers as singletons.
    //This will let us use an Async Lifestyle within the scope of said singleton.
    public class AsyncOddJobRemotingHandlerDecorator<TRequest,TResult> : IRemotingHandler<TRequest,TResult>
    {
        private readonly Func<IRemotingHandler<TRequest,TResult>> _decorateeFactory;
        private readonly Container _container;
        public AsyncOddJobRemotingHandlerDecorator(Container container, Func<IRemotingHandler<TRequest,TResult>> decorateeFactory)
        {
            _container = container;
            _decorateeFactory = decorateeFactory;
        }

        public Task<TResult> Handle(TRequest command)
        {
            using (AsyncScopedLifestyle.BeginScope(_container))
            {
                var handler = _decorateeFactory();
                return handler.Handle(command);
            }
        }
    }
}