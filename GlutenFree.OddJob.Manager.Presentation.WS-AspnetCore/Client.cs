using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore.Template;
using Microsoft.FSharp.Core;
using WebSharper;
using WebSharper.UI;
using Doc = WebSharper.UI.Doc;
using Elt = WebSharper.UI.Elt;
using Html = WebSharper.UI.Client.Html;
using Pervasives = WebSharper.JavaScript.Pervasives;

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
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
            return Html.span(Html.checkbox(useCriteriaLens), name, Html.input(criteriaLens));
        }

        public static Elt OptionSearch(string name, Var<string> criteriaLens, View<IEnumerable<string>> optionView,
            Var<bool> useCriteriaLens, Action<global::WebSharper.JavaScript.Dom.Element, global::WebSharper.JavaScript.Dom.Event> changeAction)
        {
            return Html.span(Html.checkbox(useCriteriaLens), name + ": ", Html.@select(criteriaLens, optionView, (q) => q ?? "Please Select a " + name + "..."),
                Html.@on.change(changeAction));
        }

        public static Elt DateRangeSearch(string name, Var<bool> useCriteriaLens, Var<string> beforeLens, Var<string> afterLens)
        {
            return (Html.span(Html.checkbox(useCriteriaLens), name + ": ", Html.input(beforeLens, Html.attr.type("date")), Html.input(afterLens, Html.attr.type("date"))));
        }

        public static Elt DateTimeRangeSearch(string name, Var<bool> useCriteriaLens, Var<string> beforeDateLens, Var<string> beforeTimeLens,
            Var<string> afterDateLens, Var<string> afterTimeLens)
        {
            return (Html.span(Html.checkbox(useCriteriaLens), name + ": ", Html.br(),
                Html.span( ClearableDateInput(beforeDateLens), ClearableTimeInput(beforeTimeLens)),
                Html.span( ClearableDateInput(afterDateLens), ClearableTimeInput(afterTimeLens))));
        }

        public static Elt ClearableDateInput(Var<string> dateLens)
        {
            return Html.span(Html.input(dateLens, Html.attr.type("date"), Html.style("line-height", "unset")), Html.button("Clear", () => dateLens.Value = ""));
        }

        public static Elt ClearableTimeInput(Var<string> timeLens)
        {
            return Html.span(Html.input(timeLens, Html.attr.type("time"), Html.style("line-height", "unset")), Html.button("Clear", () => timeLens.Value = ""));
        }

        public static Elt CheckableTextInput(string name,Var<bool>useInput, Var<string> valueLens)
        {
            return Html.span(Html.checkbox(useInput), name, Html.input(valueLens));
        }
        public static Elt CheckableTextInput(string name, Var<bool> useInput, Var<string> valueLens,string defaultValue)
        {
            return Html.span(Html.checkbox(useInput), name, Html.input(valueLens, Html.attr.placeholder(defaultValue)));
        }

        public static Elt CheckableNumberInput(string name, Var<bool> useInput, Var<int> valueLens,
            int defaultValue)
        {
            return Html.span(Html.checkbox(useInput), name,
                Html.input(Html.attr.type("number"), valueLens, Html.attr.placeholder(defaultValue.ToString())));
        }
    }
    [JavaScript]
    public static class JobSearchClient
    {


        public static async Task<bool> PerformJobUpdate(JobUpdateViewModel juvm)
        {
            return await WebSharper.JavaScript.Pervasives.Remote<OddJobRemotingWrapper>().Handle(juvm);
        }
        public static IControlBody Main()
        {
            
            //var myList = Var.Create<IEnumerable<string>>(OddJobRemoting.Handle());
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
                queueNames.Value = await WebSharper.JavaScript.Pervasives
                    .Remote<OddJobRemotingWrapper>().Handle(new QueueNameListRequest());
                return queueNames.Value;
            });
            criteriaFiller.Trigger();

        var methodCriteria = Var.Create<IEnumerable<string>>(new string[] {null});
            var submit = Submitter.CreateOption(criteria.View);
            var result = updateSet.Doc(juvm =>
            {
                var map = updateSet.Lens(juvm.JobGuid.ToString());
                var myUpdateItem = Var.Create(Html.div());
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
                return Html.div(Html.style("display", "grid"), Html.style("grid-template-columns", "50% 50%"), Html.style("filter",blurFilter.View), Html.style("background",disabledColor.View),
                    Html.div(
                        new Jobitem.JobItem().MethodName(juvm.MetaData.MethodName).QueueName(juvm.MetaData.Queue)
                            .Status(juvm.MetaData.Status)
                            .JobGuid(juvm.MetaData.JobId.ToString()).Doc()
                    ),
                    Html.div(
                        Html.div(
                            Html.div(ElementCreators.CheckableTextInput("Status", updateStatus, newStatus,
                                juvm.MetaData.Status)),
                            Html.div(ElementCreators.CheckableTextInput("MethodName", updateMethodName, newMethodName,
                                juvm.MetaData.MethodName)),
                            Html.div(ElementCreators.CheckableTextInput("QueueName", updateQueue, newQueue,
                                juvm.MetaData.Queue)),
                            Html.div(ElementCreators.CheckableNumberInput("MaxRetryCount", updateMaxRetryCount,
                                newMaxRetryCount,
                                juvm.MetaData.RetryParameters.MaxRetries))
                        )
                    ),
                    //TODO: This looks bad with lots of Parameters.
                    Html.div(Html.div(juvm.MetaData.JobArgs.Select((r, i) =>
                            Html.div(new Jobparameter.JobParameter().Type(r.Type).Name(r.Name).Value(r.Value)
                                .Ordinal(i.ToString()).Doc())).ToArray())),
                    Html.div(Html.style("margin-left","5%"), Html.div(jobParamUpdate)),
                    Html.button("Update", async () =>
                    {
                            if (juvm == null)
                            {
                                myUpdateItem.Value = Html.div("");
                            }

                            var success = await PerformJobUpdate(juvm);
                        if (success)
                        {
                            myUpdateItem.Value = Html.div("Updated");
                            blurFilter.Value = "blur(1px)";
                            disabledColor.Value = "lightgrey";
                            disableUpdate.Value = true;
                        }
                        else
                        {
                            myUpdateItem.Value = Html.div("Failed update");
                        }
                            
                        
                    }, Html.attr.disabled(disabledVar.View, disableUpdate.View)),
                    Html.div(myUpdateItem)
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
            var content = Html.div(Html.div(Html.style("width","100%"),
                Html.div(ElementCreators.OptionSearch("Queue Name", queueNameLens, queueNameView,useQueueLens,(r,e)=> submit.Trigger()),
                ElementCreators.TextSearch("Method Name", methodLens, useMethod),
                ElementCreators.OptionSearch("Status", statusLens, statusOptions.View, useStatus,
                    (a, b) => submit.Trigger()),
                ElementCreators.OptionSearch("Method", methodLens, methodCriteria.View, useMethod,
                    (a, b) => submit.Trigger()),
                ElementCreators.DateTimeRangeSearch("Created", useCreatedLens, createdBeforeDateLens,
                    createdBeforeTimeLens, createdAfterDateLens, createdAfterTimeLens),
                ElementCreators.DateTimeRangeSearch("Attempt", useAttemptedDTLens, lastExecutedBeforeDateLens,
                    lastExecutedBeforeTimeLens, lastExecutedAfterDateLens, lastExecutedAfterTimeLens),
                Html.div(Html.button("Search", submit.Trigger)))),
                Html.div(Html.br()),
                Html.div(Html.style("overflow-y","scroll"), results)
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
            return Html.div(
                Html.div(ElementCreators.CheckableTextInput("Type", updateParamTypeLens, newParamTypeLens,
                    juvm.MetaData.JobArgs[i].Type)),
                //TODO: Make this a TextArea for larger Parameters
                Html.div(ElementCreators.CheckableTextInput("Value", updateParamValueLens, newParamValueLens,
                    juvm.MetaData.JobArgs[i].Value))
            );
        }

        private static async Task<Elt> CreateSearchResults(FSharpOption<JobSearchCriteria> input, Var<IEnumerable<string>> methodCriteria, ListModel<string, JobUpdateViewModel> updateSet, Doc result)
        {
            if (input == null)
                return Html.div("");
            var methodOptionFuture = Pervasives.Remote<OddJobRemotingWrapper>()
                .Handle(new GetMethodsForQueueNameRequest() {QueueName = input.Value.QueueName});
            var awaitedMethodOptions = await methodOptionFuture;
            methodCriteria.Value = awaitedMethodOptions;
            var future = Pervasives.Remote<OddJobRemotingWrapper>().Handle(input.Value);
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


            return Html.div(Html.h3("Results:"), Html.br(), Html.doc(result));
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
                    return Task.FromResult("perd");
                });
            return Html.div(
                Html.input(rvInput),
                Html.button("Send", submit.Trigger),
                Html.hr(),
                Html.h4(
                    Html.attr.@class("text-muted"),
                    "The server responded:",
                    Html.div(
                        Html.attr.@class("jumbotron"),
                        Html.h1(vReversed)
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
                    return OddJobRemoting.DoSomething(input.Value);
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