using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using WebSharper.AspNetCore;
using WebSharper.Sitelets;

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSitelet<OddJobSitelet>()
                //.AddWebSharperRemoting<OddJobRemoting>()
                .AddAuthentication("WebSharper")
                .AddCookie("WebSharper", options => { });
            IntegrateSimpleInjector(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeContainer(app);
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }
            WebSharper.Core.Remoting.AddHandler(typeof(OddJobRemotingWrapper), container.GetService< OddJobRemotingWrapper>());
            //WebSharper.Core.Remoting.AddHandler(typeof(IRemotingHandler<QueueNameListRequest, string[]>),container.GetService<IRemotingHandler<QueueNameListRequest, string[]>>());
            //WebSharper.Core.Remoting.AddHandler(typeof(IRemotingHandler<JobSearchCriteria, JobMetadataResult[]>), container.GetService<IRemotingHandler<JobSearchCriteria, JobMetadataResult[]>>());
            //WebSharper.Core.Remoting.AddHandler(typeof(IRemotingHandler<GetMethodsForQueueNameRequest, string[]>), container.GetService <IRemotingHandler<GetMethodsForQueueNameRequest, string[]>>());
            //WebSharper.Core.Remoting.AddHandler(typeof(IRemotingHandler<JobUpdateViewModel, bool>), container.GetService<IRemotingHandler<JobUpdateViewModel, bool>>());

            //WebSharper.Core.Remoting.AddHandler(typeof(IRemotingHandler<,>), container.GetRequiredService(typeof(IRemotingHandler<,>)));
            //WebSharper.Core.Remoting.AddHandler(typeof(IRemotingHandler<>(OddJobRemoting), container.GetService<OddJobRemoting>());
            app.UseAuthentication()
                .UseStaticFiles()
                .UseWebSharper()
                .Run(context =>
                {
                    context.Response.StatusCode = 404;
                    return context.Response.WriteAsync("Page not found");
                });
        }

        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }

        private Container container = new Container();

        private void IntegrateSimpleInjector(IServiceCollection services)
        {
            /*services.Add(new ServiceDescriptor(typeof(OddJobRemoting), (aspnetCore) => container.GetService<OddJobRemoting>(),
                ServiceLifetime.Scoped));*/
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            /*services.AddSingleton<IControllerActivator>(
                new SimpleInjectorControllerActivator(container));
            services.AddSingleton<IViewComponentActivator>(
                new SimpleInjectorViewComponentActivator(container));
                */
            services.EnableSimpleInjectorCrossWiring(container);
            services.UseSimpleInjectorAspNetRequestScoping(container);
        }
        /*
        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeContainer(app);

            // Add custom middleware
            app.UseMiddleware<CustomMiddleware1>(container);
            app.UseMiddleware<CustomMiddleware2>(container);

            container.Verify();

            // ASP.NET default stuff here
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }*/

        private void InitializeContainer(IApplicationBuilder app)
        {
            container.Register<IJobSearchProvider>(() =>
            {
                return new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                    TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            }, Lifestyle.Scoped);
            container.Register(typeof(IRemotingHandler<,>), new[] {typeof(OddJobRemotingHandler)});
            //container.Register(typeof(OddJobRemotingHandler));
            container.RegisterDecorator(typeof(IRemotingHandler<,>), typeof(AsyncOddJobRemotingHandlerDecorator<,>));
            container.Register(typeof(OddJobRemotingWrapper));
            // Add application presentation components:
            //container.RegisterMvcControllers(app);
            //container.RegisterMvcViewComponents(app);
            // Add application services. For instance:
            //container.Register<IJobSearchProvider>(Lifestyle.Scoped);
            //container.Register<IUserService, UserService>(Lifestyle.Scoped);
            // Allow Simple Injector to resolve services from ASP.NET Core.
            container.AutoCrossWireAspNetComponents(app);
        }
    }
}
