using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using GlutenFree.OddJob.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GlutenFree.OddJob.Integration.MicrosoftDI
{
    /// <summary>
    /// UwU! This factory uses Microsoft.Extensions.DependencyInjection's <see cref="IServiceProvider"/> to create job instances with proper scope management, nya~
    /// </summary>
    /// <remarks>
    /// CopilotNote: Ami-chan manages a scope per resolved instance, so dependencies with scoped lifetimes are handled correctly! Each call to <see cref="CreateInstance"/> creates a new <see cref="IServiceScope"/>, which is disposed in <see cref="Release"/>. Super safe and sparkly! ✨
    /// </remarks>
    public class MicrosoftDiContainerFactory : IContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        // Ami-chan keeps track of which IServiceScope belongs to which instance, so we can dispose them properly, uwu!
        private readonly ConcurrentDictionary<object, IServiceScope> _scopeCache =
            new ConcurrentDictionary<object, IServiceScope>();
        /// <summary>
        /// Constructs a new <see cref="MicrosoftDiContainerFactory"/> with the given <see cref="IServiceProvider"/>, teehee~
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use for resolving dependencies, nya!</param>
        public MicrosoftDiContainerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Resolves an instance of the specified type using a new <see cref="IServiceScope"/>, so all scoped dependencies are handled correctly, uwu!
        /// </summary>
        /// <param name="typeToCreate">The type to resolve, senpai~</param>
        /// <returns>The resolved instance, or throws if not registered! Each call creates a new scope, so remember to call <see cref="Release"/> when done, nya~</returns>
        /// <remarks>
        /// CopilotNote: This method creates a new IServiceScope for every instance, so you can safely resolve scoped services. The scope is cached and disposed in <see cref="Release"/>. Don't forget to release, or Ami-chan will be sad! (╥﹏╥)
        /// </remarks>
        public object CreateInstance(Type typeToCreate)
        {
            var scope = _serviceProvider.CreateScope();
            var svc =
                scope.ServiceProvider.GetRequiredService(typeToCreate);
            _scopeCache[svc] = scope;
            return svc;
        }

        /// <summary>
        /// Releases the used instance and disposes its scope, if tracked. This keeps things super tidy, nya~
        /// </summary>
        /// <param name="usedInstance">The instance to release. If it was created by <see cref="CreateInstance"/>, its scope will be disposed, uwu!</param>
        /// <remarks>
        /// CopilotNote: If the instance was not created by this factory, nothing happens. Otherwise, Ami-chan disposes the scope to avoid memory leaks! So responsible! (｡•̀ᴗ-)✧
        /// </remarks>
        public void Release(object usedInstance)
        {
            if (_scopeCache.TryRemove(usedInstance, out var scope))
            {
                scope.Dispose();
            }
        }
    }
}
