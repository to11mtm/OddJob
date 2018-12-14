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
    public static class JSHelpers
    {
       public static string DateToString(DateTime date) =>
            $"{date.Year}-{date.Month.ToString().PadLeft(2, '0')}-{date.Day.ToString().PadLeft(2, '0')}";

        public static string TimeToString(DateTime time) =>
            $"{time.Hour}:{time.Minute}";
    }
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

        public static Elt DateRangeSearch(string name, Var<bool> useCriteriaLens,Var<string> beforeLens, Var<string> afterLens)
        {
            return (div(checkbox(useCriteriaLens),name + ": ", input(beforeLens, attr.type("date")), input(afterLens,attr.type("date"))));
        }

        public static Elt DateTimeRangeSearch(string name, Var<bool> useCriteriaLens, Var<string> beforeDateLens, Var<string> beforeTimeLens,
            Var<string> afterDateLens, Var<string>afterTimeLens)
        {
            return (div(style("float", "left"),checkbox(useCriteriaLens), name + ": ", br(),
                div(style("float","left"), ClearableDateInput(beforeDateLens),ClearableTimeInput(beforeTimeLens)),
                div(style("float", "left"),ClearableDateInput(afterDateLens), ClearableTimeInput(afterTimeLens))));
        }

        public static Elt ClearableDateInput(Var<string> dateLens)
        {
            return div(input(dateLens, attr.type("date")), button("Clear", () => dateLens.Value = ""));
        }
        public static Elt ClearableTimeInput(Var<string> timeLens)
        {
            return div(input(timeLens, attr.type("time")), button("Clear", () => timeLens.Value = ""));
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
            var dummyQueueCriteriaFiller = Var.Create("");
            var criteriaFiller = Submitter.CreateOption(dummyQueueCriteriaFiller.View);

            var queueNames = Var.Create<IEnumerable<string>>(new string[] {null});
            var queueNameView = criteriaFiller.View.MapAsync(async input =>
            {
                queueNames.Value=  await Remoting.GetQueueNameList();
                return queueNames.Value;
            });
            criteriaFiller.Trigger();
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
            var useCreatedLens = criteria.Lens(q => q.useCreatedDate, (a, b) =>
            {
                a.useCreatedDate = b;
                return a;
            });
            var createdBeforeDateLens = criteria.Lens(q => q.createdBefore, (a, b) =>
            {
                a.createdBefore = b;
                return a;
            });
            var createdAfterDateLens = criteria.Lens(q => q.createdAfter, (a, b) =>
            {
                a.createdBefore = b;
                return a;
            });
            var createdBeforeTimeLens = criteria.Lens(q => q.createdBeforeTime, (a, b) =>
            {
                a.createdBeforeTime = b;
                return a;
            });
            var createdAfterTimeLens = criteria.Lens(q => q.createdAfterTime, (a, b) =>
            {
                a.createdAfterTime = b;
                return a;
            });
            var useAttemptedDTLens = criteria.Lens(q => q.useLastAttemptDate, (a, b) =>
            {
                a.useLastAttemptDate = b;
                return a;
            });
            var lastExecutedBeforeTimeLens = criteria.Lens(q => q.attemptedBeforeTime, (a, b) =>
            {
                a.attemptedBeforeTime = b;
                return a;
            });
            var lastExecutedBeforeDateLens = criteria.Lens(q => q.attemptedBeforeDate, (a, b) =>
            {
                a.attemptedBeforeDate = b;
                return a;
            });
            var lastExecutedAfterTimeLens = criteria.Lens(q => q.attemptedAfterTime, (a, b) =>
            {
                a.attemptedAfterTime = b;
                return a;
            });
            var lastExecutedAfterDateLens = criteria.Lens(q => q.attemptedAfterDate, (a, b) =>
            {
                a.attemptedAfterDate = b;
                return a;
            });
           /* WebSharper.Html.Client.Operators.OnAfterRender(
                f: FSharpConvert.Fun<Datepicker>(d =>
                {
                    d.SetDate(DateTime.Now.ToShortDateString());
                }),
                w: createdFilter);*/
            var content = div(
                    div("Queue Name: ",@select(queueNameLens, queueNameView, (q) => q ?? "Please Select a Queue..."),on.change((r,e)=> submit.Trigger()), attr.name("queueNameSelect")),
                    TextSearch("Method Name", methodLens,useMethod),
                    OptionSearch("Status", statusLens, statusOptions.View, useStatus, (a,b)=>submit.Trigger()),
                    OptionSearch("Method", methodLens, methodCriteria.View, useMethod,(a,b)=> submit.Trigger()),
                    DateTimeRangeSearch("Created", useCreatedLens, createdBeforeDateLens, createdBeforeTimeLens, createdAfterDateLens, createdAfterTimeLens),
                    DateTimeRangeSearch("Attempt", useAttemptedDTLens, lastExecutedBeforeDateLens, lastExecutedBeforeTimeLens, lastExecutedAfterDateLens, lastExecutedAfterTimeLens),
                    br(),
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