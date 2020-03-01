using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OddJob.SignalR.Test
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            
            //using (app.ApplicationServices.CreateScope().ServiceProvider.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                
                endpoints.MapHub<GroupHub>("/testhub");
                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync("Hello World!");
                    });
            });
        }
    }

    public class EndPt
    {
        public void Message(string msg)
        {
            
        }
    }

    public class TypedRClient<TClient, THub>
    {
        private TClient _clientReceiver;
        public TypedRClient(TClient clientReceiver)
        {

            _clientReceiver = clientReceiver;
            var builtHubConection = new HubConnectionBuilder()
                .Build();
            
            //builtHubConection.On()
            var methods =
                typeof(TClient).GetMethods(
                    BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods)
            {
                var delgt = CreateExecuteDelegate(methodInfo);
                builtHubConection.On(methodInfo.Name,
                    methodInfo.GetParameters().Select(r => r.ParameterType)
                        .ToArray(), pars => delgt(_clientReceiver, pars));
            }

            var svMethods =
                typeof(THub).GetMethods(
                    BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in svMethods)
            {
               // builtHubConection.SendAsync()
            }
        }

        public static Func<object[],Task> CreateSendAsync(HubConnection conn,
            MethodInfo mi)
        {
          //  conn.SendAsync().SendCoreAsync()
            var hub = Expression.Constant(conn);
            var varExprs = mi.GetParameters().Select(p =>
                Expression.Variable(p.ParameterType, p.Name + "i"));
            return null;
        }

        public static Func<TClient,object[], Task> CreateExecuteDelegate(MethodInfo mi)
        {
            var inputExpr = Expression.Parameter(typeof(TClient), "client");
            var input = Expression.Parameter(typeof(object[]), "inputPars");
            
            var pars = mi.GetParameters();
            var parGrab = new List<Expression>();
            for (int i = 0; i < pars.Length; i++)
            {
                var toAdd =
                    Expression.ArrayIndex(input, Expression.Constant(i));
                parGrab.Add(toAdd);
            }

            var methodCallExpr = Expression.Call(inputExpr, mi, parGrab);

            return Expression.Lambda<Func<TClient, object[], Task>>(methodCallExpr,
                new[] {inputExpr, input}).Compile();

        }
    }

    public interface IFoo
    {
        void Derp(string derp);
    }
    
    public interface IBar
    {
        Task Derp(string derp);
        Task VFoo(string v);
    }

    public class GroupHub : Hub<IBar>, IFoo
    {
        private IServiceProvider _serviceProvider;

        public GroupHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }



        public List<string> GetGroups()
        {
            return null;
        }

        public void Derp(string payload)
        {
            System.Console.WriteLine("lol");
            this.Clients.Caller.VFoo("ok");
            //return Task.CompletedTask;
        }
    }
}