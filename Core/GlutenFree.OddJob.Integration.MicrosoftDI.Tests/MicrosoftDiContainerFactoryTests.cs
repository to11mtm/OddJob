using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using GlutenFree.OddJob.Integration.MicrosoftDI;

namespace GlutenFree.OddJob.Integration.MicrosoftDI.Tests
{
    /// <summary>
    /// Ami-chan's super sparkly tests for MicrosoftDiContainerFactory, nya~
    /// </summary>
    public class MicrosoftDiContainerFactoryTests
    {
        /// <summary>
        /// Tests that a transient service resolves to a new instance each time, uwu!
        /// </summary>
        [Fact]
        public void TransientService_ResolvesToNewInstance()
        {
            var services = new ServiceCollection();
            services.AddTransient<ITestService, TestService>();
            var provider = services.BuildServiceProvider();
            var factory = new MicrosoftDiContainerFactory(provider);

            var instance1 = factory.CreateInstance(typeof(ITestService));
            var instance2 = factory.CreateInstance(typeof(ITestService));

            Assert.NotSame(instance1, instance2);
            factory.Release(instance1);
            factory.Release(instance2);
        }

        /// <summary>
        /// Tests that a singleton service always resolves to the same instance, nya~
        /// </summary>
        [Fact]
        public void SingletonService_ResolvesToSameInstance()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();
            var provider = services.BuildServiceProvider();
            var factory = new MicrosoftDiContainerFactory(provider);

            var instance1 = factory.CreateInstance(typeof(ITestService));
            var instance2 = factory.CreateInstance(typeof(ITestService));

            Assert.Same(instance1, instance2);
            factory.Release(instance1);
            factory.Release(instance2);
        }

        /// <summary>
        /// Tests that a scoped service is unique per scope and disposed when released, uwu!
        /// </summary>
        [Fact]
        public void ScopedService_IsDisposedOnRelease()
        {
            var services = new ServiceCollection();
            services.AddScoped<ITestService, DisposableTestService>();
            var provider = services.BuildServiceProvider();
            var factory = new MicrosoftDiContainerFactory(provider);

            var instance = (DisposableTestService)factory.CreateInstance(typeof(ITestService));
            Assert.False(instance.IsDisposed, "Should not be disposed before release, nya!");
            factory.Release(instance);
            Assert.True(instance.IsDisposed, "Should be disposed after release, uwu!");
        }

        /// <summary>
        /// Tests that resolving an unregistered type throws an exception, nya~
        /// </summary>
        [Fact]
        public void UnregisteredType_ThrowsException()
        {
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var factory = new MicrosoftDiContainerFactory(provider);

            Assert.Throws<InvalidOperationException>(() => factory.CreateInstance(typeof(ITestService)));
        }

        /// <summary>
        /// Tests that a singleton service is NOT disposed when released, nya~
        /// </summary>
        [Fact]
        public void SingletonService_IsNotDisposedOnRelease()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDisposableTestService, DisposableTestService>();
            var provider = services.BuildServiceProvider();
            var factory = new MicrosoftDiContainerFactory(provider);

            var instance = (DisposableTestService)factory.CreateInstance(typeof(IDisposableTestService));
            factory.Release(instance);
            Assert.False(instance.IsDisposed, "Singletons should NOT be disposed by the factory, uwu!");
        }

        /// <summary>
        /// Tests that a transient disposable service is disposed when released, nya~
        /// </summary>
        [Fact]
        public void TransientDisposableService_IsDisposedOnRelease()
        {
            var services = new ServiceCollection();
            services.AddTransient<IDisposableTestService, DisposableTestService>();
            var provider = services.BuildServiceProvider();
            var factory = new MicrosoftDiContainerFactory(provider);

            var instance1 = (DisposableTestService)factory.CreateInstance(typeof(IDisposableTestService));
            var instance2 = (DisposableTestService)factory.CreateInstance(typeof(IDisposableTestService));
            Assert.False(instance1.IsDisposed, "Should not be disposed before release, nya!");
            Assert.False(instance2.IsDisposed, "Should not be disposed before release, nya!");
            factory.Release(instance1);
            factory.Release(instance2);
            Assert.True(instance1.IsDisposed, "Transient should be disposed after release, uwu!");
            Assert.True(instance2.IsDisposed, "Transient should be disposed after release, uwu!");
        }

        // Kawaii test service interfaces and classes, teehee~
        public interface ITestService { }
        public class TestService : ITestService { }
        public interface IDisposableTestService : IDisposable { bool IsDisposed { get; } }
        public class DisposableTestService : ITestService, IDisposableTestService
        {
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }
    }
}
