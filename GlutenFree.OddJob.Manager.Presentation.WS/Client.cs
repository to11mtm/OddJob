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
using Elt = WebSharper.UI.Elt;
using Html = WebSharper.UI.Client.Html;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    [JavaScript]
    public static class JobSearchClient
    {
        public static Elt TextSearch(string name, Var<string> criteriaLens, Var<bool> useCriteriaLens)
        {
            return div(checkbox(useCriteriaLens), name, input(criteriaLens));
        }

        public static Elt OptionSearch(string name, Var<string> criteriaLens, View<IEnumerable<string>> optionView,
            Var<bool> useCriteriaLens, Action<WebSharper.JavaScript.Dom.Element,WebSharper.JavaScript.Dom.Event> changeAction)
        {
            return div(checkbox(useCriteriaLens), name + ": ", @select(criteriaLens, optionView, (q) => q ?? "Please Select a " + name +"..."),
                on.change(changeAction));
        }

        public static IControlBody Main()
        {
            
            //var myList = Var.Create<IEnumerable<string>>(Remoting.GetQueueNameList());
            var criteria = Var.Create(new JobSearchCriteria());
            var statusLens = criteria.Lens(q => q.Status, (a, b) =>
            {
                a.Status = b;
                return a;
            });
            var useStatus = criteria.Lens(q => q.UseStatus, (a, b) =>
            {
                a.UseStatus = b;
                return a;
            });
            var methodLens = criteria.Lens(q => q.MethodName, (a, b) =>
            {
                a.MethodName = b;
                return a;
            });
            var useMethod = criteria.Lens(q => q.UseMethod, (a, b) =>
            {
                a.UseMethod = b;
                return a;
            });
            var statusOptions = Var.Create<IEnumerable<string>>(new[]
            {
                null, "Processed", "New", "Failed",
                "Retry", "InProgress", "Inserting"
            });
        var methodCriteria = Var.Create<IEnumerable<string>>(new string[] {null});
            var submit = Submitter.CreateOption(criteria.View);
            var results = submit.View.MapAsync(async input =>
            {
                if (input == null)
                    return div("");
                var methodOptionFuture = Remoting.GetMethods(input.Value.QueueName);
                var awaitedMethodOptions = await methodOptionFuture;
                methodCriteria.Value = awaitedMethodOptions;
                var future = Remoting.SearchCriteria(input.Value);
                var awaitedFuture = await future;
                var result = awaitedFuture.Select(q => new Jobitem.JobItem().MethodName(q.MethodName).QueueName(q.Queue).Status(q.Status).JobGuid(q.JobId.ToString())
                    .JobParameter(
                        ul(q.JobArgs.Select((r, i) =>
                            new Jobparameter.JobParameter().Type(r.Type).Name(r.Name).Value(r.Value)
                                .Ordinal(i.ToString()).Doc()).ToArray())).Doc()
                ).ToArray();
                return div(h3("Results:"),br(), doc(ul(result)));
            });

            var queueNameLens = criteria.Lens(q => q.QueueName, (a, b) =>
            {
                a.QueueName = b;
                return a;
            });
            
            
            

            var content = div(
                    div("Queue Name: ",@select(queueNameLens, new[]{null,"console","counter"}, (q) => q ?? "Please Select a Queue..."),on.change((r,e)=> submit.Trigger()), attr.name("queueNameSelect")),
                    TextSearch("Method Name", methodLens,useMethod),
                    OptionSearch("Status", statusLens, statusOptions.View, useStatus, (a,b)=>submit.Trigger()),
                    OptionSearch("Method", methodLens, methodCriteria.View, useMethod,(a,b)=> submit.Trigger()),
                    button("Search", submit.Trigger),
                    div(results)
                );
                
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