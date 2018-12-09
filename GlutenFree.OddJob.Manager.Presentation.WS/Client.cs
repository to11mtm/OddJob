using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GlutenFree.OddJob.Manager.Presentation.WS.Template;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using Microsoft.FSharp.Core;
using WebSharper;
using WebSharper.JavaScript;
using WebSharper.Sitelets;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;
using Html = WebSharper.UI.Client.Html;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    [JavaScript]
    public static class JobSearchClient
    {
        public static IControlBody Main()
        {
            var jobs = new ListModel<Guid, JobSearchCriteria>(job => job.JobGuid.GetValueOrDefault(Guid.Empty));
            var criteria = Var.Create(new JobSearchCriteria());
            var testList = new ListModel<string, string>(s => s);
            var submit = Submitter.CreateOption(criteria.View);
            var results = submit.View.MapAsync(async input =>
            {
                if (input == null)
                    return div("");
                var future = Remoting.SearchCriteria(input.Value.QueueName, input.Value.MethodName);
                var awaitedFuture = await future;
                //awaitedFuture.Select(q => new Jobitem.JobItem().MethodName(q.MethodName).QueueName(q.Queue).Doc());
                var result = awaitedFuture.Select(q => new Jobitem.JobItem().MethodName(q.MethodName).QueueName(q.Queue).Status(q.Status).JobGuid(q.JobId.ToString())
                    .JobParameter(
                        ul(q.JobArgs.Select((r, i) =>
                            new Jobparameter.JobParameter().Type(r.Type).Name(r.Name).Value(r.Value)
                                .Ordinal(i.ToString()).Doc()).ToArray())).Doc()
                ).ToArray(); //WebSharper.TypedJson.Serialize(awaitedFuture);
                return div(h3("Results:"),br(), doc(ul(result)));
            });

            var var1 = criteria.Lens(q => q.QueueName, (a, b) =>
            {
                a.QueueName = b;
                return a;
            });
            var var2 = criteria.Lens(q => q.MethodName, (a, b) =>
            {
                a.MethodName = b;
                return a;
            });
            var content = div(
                    div("Queue Name: ",input(var1, attr.name("queueName"))),
                    div("Method Name: ",input(var2, attr.name("methodName"))),
                    button("Search", submit.Trigger),
                    div(results)
                );
                

                /*new Jobsearch.Main()
                    .SearchMethodName(var1)
                    .SearchQueueName(var2)
                    .Search(x => { testList.View.MapAsync(input=> Remoting.SearchCriteria(var1.Value, var2.Value)); }).Doc());*/
            return content;
        }
    }
    [JavaScript]
    public static class Client
    {
        static public IControlBody Main()
        {

            var rvInput = Var.Create(new JobSearchCriteria());
            var submit = Submitter.CreateOption(rvInput.View);
            var vReversed =
                submit.View.MapAsync(input =>
                {
                    if (input == null)
                        return Task.FromResult("");
                    return Remoting.DoSomething(input.Value);
                });
            return div(
                input(rvInput),
                button("Send", submit.Trigger),
                hr(),
                h4(
                    attr.@class("text-muted"),
                    "The server responded:",
                    div(
                        attr.@class("jumbotron"),
                        h1(vReversed)
                    )
                )
            );
        }
        /*static public IControlBody Main()
        {
            var rvInput = Var.Create("");
            var submit = Submitter.CreateOption(rvInput.View);
            var vReversed =
                submit.View.MapAsync(input =>
                {
                    if (input == null)
                        return Task.FromResult("");
                    return Remoting.DoSomething(input.Value);
                });
            return div(
                input(rvInput),
                button("Send", submit.Trigger),
                hr(),
                h4(
                    attr.@class("text-muted"),
                    "The server responded:",
                    div(
                        attr.@class("jumbotron"),
                        h1(vReversed)
                    )
                )
            );
        }*/
    }
}