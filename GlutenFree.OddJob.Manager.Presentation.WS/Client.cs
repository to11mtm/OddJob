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
using Doc = WebSharper.UI.Doc;
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
            return span(input(dateLens, attr.type("date"), style("line-height", "unset")), button("Clear", () => dateLens.Value = ""));
        }

        public static Elt ClearableTimeInput(Var<string> timeLens)
        {
            return span(input(timeLens, attr.type("time"), style("line-height", "unset")), button("Clear", () => timeLens.Value = ""));
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


        public static async Task<bool> PerformJobUpdate(JobUpdateViewModel juvm)
        {
            return await Remoting.UpdateJob(juvm);
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
                var myUpdateItem = Var.Create(div());
                var jobParamUpdate = juvm.UpdateData.ParamUpdates.Select((updateDateParamUpdate,i) =>
                {
                    return CreateParamterUpdateElement(map, i, juvm);
                }).ToArray();
                var newMethodName = map.Lens(uvm => uvm.UpdateData.NewMethodName, (a, b) =>
                {
                    a.UpdateData.NewMethodName = b;
                    return a;
                });
                var updateMethodName = map.Lens(uvm => uvm.UpdateData.UpdateMethodName, (a, b) =>
                {
                    a.UpdateData.UpdateMethodName = b;
                    return a;
                });
                var updateStatus = map.Lens(uvm => uvm.UpdateData.UpdateStatus, (a, b) =>
                {
                    a.UpdateData.UpdateStatus = b;
                    return a;
                });
                var newStatus = map.Lens(uvm => uvm.UpdateData.NewStatus, (a, b) =>
                {
                    a.UpdateData.NewStatus = b;
                    return a;
                });
                var updateQueue = map.Lens(uvm => uvm.UpdateData.UpdateQueueName, (a, b) =>
                {
                    a.UpdateData.UpdateQueueName = b;
                    return a;
                });
                var newQueue = map.Lens(uvm => uvm.UpdateData.NewQueueName, (a, b) =>
                {
                    a.UpdateData.NewQueueName = b;
                    return a;
                });
                var updateMaxRetryCount = map.Lens(uvm => uvm.UpdateData.UpdateRetryCount, (a, b) =>
                {
                    a.UpdateData.UpdateRetryCount = b;
                    return a;
                });
                var newMaxRetryCount = map.Lens(uvm => uvm.UpdateData.NewMaxRetryCount, (a, b) =>
                {
                    a.UpdateData.NewMaxRetryCount = b;
                    return a;
                });
                var blurFilter = Var.Create("");
                var disableUpdate = Var.Create(false);
                var disabledVar = Var.Create("Disabled");
                var disabledColor = Var.Create("");
                return div(style("display", "grid"), style("grid-template-columns", "50% 50%"), style("filter",blurFilter.View), style("background",disabledColor.View),
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
                    button("Update", async () =>
                    {
                            if (juvm == null)
                            {
                                myUpdateItem.Value = div("");
                            }

                            var success = await PerformJobUpdate(juvm);
                        if (success)
                        {
                            myUpdateItem.Value = div("Updated");
                            blurFilter.Value = "blur(1px)";
                            disabledColor.Value = "lightgrey";
                            disableUpdate.Value = true;
                        }
                        else
                        {
                            myUpdateItem.Value = div("Failed update");
                        }
                            
                        
                    }, attr.disabled(disabledVar.View, disableUpdate.View)),
                    div(myUpdateItem)
                );
            });
            var results = submit.View.MapAsync(async input =>
            {
                return await CreateSearchResults(input, methodCriteria, updateSet, result);
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
                div(style("overflow-y","scroll"), results)
            );
                
            return content;
        }

        private static Elt CreateParamterUpdateElement(Var<JobUpdateViewModel> map, int i, JobUpdateViewModel juvm)
        {
            var updateParamTypeLens = map.Lens(
                uvm => uvm.UpdateData.ParamUpdates[i].UpdateParamType,
                (a, b) =>
                {
                    a.UpdateData.ParamUpdates[i].UpdateParamType = b;
                    return a;
                });
            var newParamTypeLens = map.Lens(
                uvm => uvm.UpdateData.ParamUpdates[i].NewParamType,
                (a, b) =>
                {
                    a.UpdateData.ParamUpdates[i].NewParamType = b;
                    return a;
                });
            var updateParamValueLens = map.Lens(
                uvm => uvm.UpdateData.ParamUpdates[i].UpdateParamValue,
                (a, b) =>
                {
                    a.UpdateData.ParamUpdates[i].UpdateParamValue = b;
                    return a;
                });
            var newParamValueLens = map.Lens(
                uvm => uvm.UpdateData.ParamUpdates[i].NewParamValue,
                (a, b) =>
                {
                    a.UpdateData.ParamUpdates[i].NewParamValue = b;
                    return a;
                });
            return div(
                ElementCreators.CheckableTextInput("Type", updateParamTypeLens, newParamTypeLens,
                    juvm.MetaData.JobArgs[i].Type),
                ElementCreators.CheckableTextInput("Value", updateParamValueLens, newParamValueLens,
                    juvm.MetaData.JobArgs[i].Value)
            );
        }

        private static async Task<Elt> CreateSearchResults(FSharpOption<JobSearchCriteria> input, Var<IEnumerable<string>> methodCriteria, ListModel<string, JobUpdateViewModel> updateSet, Doc result)
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
                    UpdateData = new UpdateForJob()
                    {
                        JobGuid = q.JobId,
                        OldStatus = q.Status,
                        ParamUpdates = q.JobArgs.Select(r=> new UpdateForParam(){ ParamOrdinal = r.Ordinal}).ToArray()
                    }
                };
            }));


            return div(h3("Results:"), br(), doc(result));
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