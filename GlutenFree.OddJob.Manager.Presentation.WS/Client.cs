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
using WebSharper.JavaScript.Dom;
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
    public static class ElementCreators
    {
        public static Elt TextSearch(string name, Var<string> criteriaLens, Var<bool> useCriteriaLens)
        {
            return span(checkbox(useCriteriaLens), name, input(criteriaLens));
        }

        public static Elt OptionSearch(string name, Var<string> criteriaLens, View<IEnumerable<string>> optionView,
            Var<bool> useCriteriaLens, Action<WebSharper.JavaScript.Dom.Element, WebSharper.JavaScript.Dom.Event> changeAction)
        {
            return span(checkbox(useCriteriaLens), name + ": ", @select(criteriaLens, optionView, (q) => q ?? "Please Select a " + name + "..."),
                @on.change(changeAction));
        }

        public static Elt DateRangeSearch(string name, Var<bool> useCriteriaLens, Var<string> beforeLens, Var<string> afterLens)
        {
            return (span(checkbox(useCriteriaLens), name + ": ", input(beforeLens, attr.type("date")), input(afterLens, attr.type("date"))));
        }

        public static Elt DateTimeRangeSearch(string name, Var<bool> useCriteriaLens, Var<string> beforeDateLens, Var<string> beforeTimeLens,
            Var<string> afterDateLens, Var<string> afterTimeLens)
        {
            return (span(checkbox(useCriteriaLens), name + ": ", br(),
                span( ClearableDateInput(beforeDateLens), ClearableTimeInput(beforeTimeLens)),
                span( ClearableDateInput(afterDateLens), ClearableTimeInput(afterTimeLens))));
        }

        public static Elt ClearableDateInput(Var<string> dateLens)
        {
            return span(input(dateLens, attr.type("date")), button("Clear", () => dateLens.Value = ""));
        }

        public static Elt ClearableTimeInput(Var<string> timeLens)
        {
            return span(input(timeLens, attr.type("time")), button("Clear", () => timeLens.Value = ""));
        }

        public static Elt CheckableTextInput(string name,Var<bool>useInput, Var<string> valueLens)
        {
            return span(checkbox(useInput), name, input(valueLens));
        }
        public static Elt CheckableTextInput(string name, Var<bool> useInput, Var<string> valueLens,string defaultValue)
        {
            return span(checkbox(useInput), name, input(valueLens, attr.placeholder(defaultValue)));
        }

        public static Elt CheckableNumberInput(string name, Var<bool> useInput, Var<int> valueLens,
            int defaultValue)
        {
            return span(checkbox(useInput), name,
                input(attr.type("number"), valueLens, attr.placeholder(defaultValue.ToString())));
        }
    }
    [JavaScript]
    public static class JobSearchClient
    {

        public static Elt BuildUpdateForJob(JobUpdateViewModel juvm)
        {
            return div();
        }
        
        public static IControlBody Main()
        {
            
            //var myList = Var.Create<IEnumerable<string>>(Remoting.GetQueueNameList());
            var criteria = Var.Create(new JobSearchCriteria());
            var useQueueLens = criteria.Lens(q => q.UseQueue, (a, b) =>
            {
                a.UseQueue = b;
                return a;
            });
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
            var updateSet = new ListModel<string, JobUpdateViewModel>(q => q.JobGuid.ToString());
            var queueNames = Var.Create<IEnumerable<string>>(new string[] {null});
            var queueNameView = criteriaFiller.View.MapAsync(async input =>
            {
                queueNames.Value=  await Remoting.GetQueueNameList();
                return queueNames.Value;
            });
            criteriaFiller.Trigger();

        var methodCriteria = Var.Create<IEnumerable<string>>(new string[] {null});
            var submit = Submitter.CreateOption(criteria.View);
            var result = updateSet.Doc(juvm =>
            {
                var map = updateSet.Lens(juvm.JobGuid.ToString());
                var updateSubmitter = Submitter.CreateOption(map.View);
                var updateResult = updateSubmitter.View.MapAsync(async input =>
                {
                    if (input == null)
                    {
                        return div("");
                    }
                    var success = await Remoting.UpdateJob(input.Value);
                    return div(success ? "Updated" : "Failed update");
                });
                var jobParamUpdate = juvm.UpdateDate.ParamUpdates.Select((updateDateParamUpdate,i) =>
                {
                    var updateParamTypeLens = map.Lens(
                        uvm => uvm.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].UpdateParamType,
                        (a, b) =>
                        {
                            a.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].UpdateParamType = b;
                            return a;
                        });
                    var newParamTypeLens = map.Lens(
                        uvm => uvm.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].NewParamType,
                        (a, b) =>
                        {
                            a.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].NewParamType = b;
                            return a;
                        });
                    var updateParamValueLens = map.Lens(
                        uvm => uvm.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].UpdateParamValue,
                        (a, b) =>
                        {
                            a.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].UpdateParamValue = b;
                            return a;
                        });
                    var newParamValueLens = map.Lens(
                        uvm => uvm.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].NewParamValue,
                        (a, b) =>
                        {
                            a.UpdateDate.ParamUpdates[updateDateParamUpdate.Key].NewParamValue = b;
                            return a;
                        });
                    return div(
                        ElementCreators.CheckableTextInput("Type", updateParamTypeLens, newParamTypeLens,
                            juvm.MetaData.JobArgs[i].Type),
                        ElementCreators.CheckableTextInput("Value",updateParamValueLens, newParamValueLens, juvm.MetaData.JobArgs[i].Value)
                    );
                }).ToArray();
                var newMethodName = map.Lens(uvm => uvm.UpdateDate.NewMethodName, (a, b) =>
                {
                    a.UpdateDate.NewMethodName = b;
                    return a;
                });
                var updateMethodName = map.Lens(uvm => uvm.UpdateDate.UpdateMethodName, (a, b) =>
                {
                    a.UpdateDate.UpdateMethodName = b;
                    return a;
                });
                var updateStatus = map.Lens(uvm => uvm.UpdateDate.UpdateStatus, (a, b) =>
                {
                    a.UpdateDate.UpdateStatus = b;
                    return a;
                });
                var newStatus = map.Lens(uvm => uvm.UpdateDate.NewStatus, (a, b) =>
                {
                    a.UpdateDate.NewStatus = b;
                    return a;
                });
                var updateQueue = map.Lens(uvm => uvm.UpdateDate.UpdateQueueName, (a, b) =>
                {
                    a.UpdateDate.UpdateQueueName = b;
                    return a;
                });
                var newQueue = map.Lens(uvm => uvm.UpdateDate.NewQueueName, (a, b) =>
                {
                    a.UpdateDate.NewQueueName = b;
                    return a;
                });
                var updateMaxRetryCount = map.Lens(uvm => uvm.UpdateDate.UpdateRetryCount, (a, b) =>
                {
                    a.UpdateDate.UpdateRetryCount = b;
                    return a;
                });
                var newMaxRetryCount = map.Lens(uvm => uvm.UpdateDate.NewMaxRetryCount, (a, b) =>
                {
                    a.UpdateDate.NewMaxRetryCount = b;
                    return a;
                });
                return div(style("display", "grid"), style("grid-template-columns", "50% 50%"),
                    div(
                        new Jobitem.JobItem().MethodName(juvm.MetaData.MethodName).QueueName(juvm.MetaData.Queue)
                            .Status(juvm.MetaData.Status)
                            .JobGuid(juvm.MetaData.JobId.ToString()).Doc()
                    ),
                    div(
                        div(
                            div(ElementCreators.CheckableTextInput("Status", updateStatus, newStatus,
                                juvm.MetaData.Status)),
                            div(ElementCreators.CheckableTextInput("MethodName", updateMethodName, newMethodName,
                                juvm.MetaData.MethodName)),
                            div(ElementCreators.CheckableTextInput("QueueName", updateQueue, newQueue,
                                juvm.MetaData.Queue)),
                            div(ElementCreators.CheckableNumberInput("MaxRetryCount", updateMaxRetryCount,
                                newMaxRetryCount,
                                juvm.MetaData.RetryParameters.MaxRetries))
                        )
                    ),
                    div(div(juvm.MetaData.JobArgs.Select((r, i) =>
                            span(new Jobparameter.JobParameter().Type(r.Type).Name(r.Name).Value(r.Value)
                                .Ordinal(i.ToString()).Doc())).ToArray())),
                    div(jobParamUpdate),
                    button("Update", () =>
                    {
                        updateSubmitter.Trigger();
                    }),
                    div(updateResult)
                );
            });
            var results = submit.View.MapAsync(async input =>
            {
                if (input == null)
                    return div("");
                var methodOptionFuture = Remoting.GetMethods(input.Value.QueueName);
                var awaitedMethodOptions = await methodOptionFuture;
                methodCriteria.Value = awaitedMethodOptions;
                var future = Remoting.SearchCriteria(input.Value);
                var awaitedFuture = await future;



               
                updateSet.Set(awaitedFuture.Select(q =>
                {
                    return new JobUpdateViewModel()
                    {
                        JobGuid = q.JobId,
                        MetaData = q,
                        UpdateDate = new UpdateForJob()
                        {
                            JobGuid = q.JobId,
                            OldStatus = q.Status,
                            ParamUpdates = q.JobArgs.OrderBy(a => a.Ordinal)
                                .Select((r, i) => new
                                {
                                    key = i.ToString(),
                                    value = new UpdateForParam()
                                }).ToDictionary(r => r.key, s => s.value)

                        }
                    };
                }));
                
                
                return div(h3("Results:"),br(), doc(result));
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
            var content = div(div(style("width","100%"),
                div(ElementCreators.OptionSearch("Queue Name", queueNameLens, queueNameView,useQueueLens,(r,e)=> submit.Trigger()),
                ElementCreators.TextSearch("Method Name", methodLens, useMethod),
                ElementCreators.OptionSearch("Status", statusLens, statusOptions.View, useStatus,
                    (a, b) => submit.Trigger()),
                ElementCreators.OptionSearch("Method", methodLens, methodCriteria.View, useMethod,
                    (a, b) => submit.Trigger()),
                ElementCreators.DateTimeRangeSearch("Created", useCreatedLens, createdBeforeDateLens,
                    createdBeforeTimeLens, createdAfterDateLens, createdAfterTimeLens),
                ElementCreators.DateTimeRangeSearch("Attempt", useAttemptedDTLens, lastExecutedBeforeDateLens,
                    lastExecutedBeforeTimeLens, lastExecutedAfterDateLens, lastExecutedAfterTimeLens),
                div(button("Search", submit.Trigger)))),
                div(br()),
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