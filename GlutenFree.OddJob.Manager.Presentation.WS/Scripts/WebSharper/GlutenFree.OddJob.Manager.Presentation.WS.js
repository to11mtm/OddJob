(function()
{
 "use strict";
 var Global,GlutenFree,OddJob,Manager,Presentation,WS,JSHelpers,ElementCreators,JobSearchClient,Client,WebSharper,Obj,JobSearchCriteria,UpdateForJob,UpdateForParam,JobRetryParameters,JobParameterDto,JobUpdateViewModel,JobMetadataResult,Template,Jobitem,Vars,UI,Templating,Runtime,Server,TemplateInstance,Instance,JobItem,Vars$1,Instance$1,Jobparameter,Vars$2,Instance$2,JobParameter,Vars$3,Instance$3,Jobsearch,Vars$4,Instance$4,ListItem,Vars$5,Instance$5,Main,Vars$6,Instance$6,Main$1,Vars$7,Instance$7,Editablejobitem,Vars$8,Instance$8,EditableJobItem,Vars$9,Instance$9,Searchoption,Vars$10,Instance$10,SearchOption,Vars$11,Instance$11,GlutenFree$OddJob$Manager$Presentation$WS_Templates,Strings,Doc,AttrProxy,View,List,AttrModule,Var$1,Submitter,ListModel,Remoting,AjaxRemotingProvider,Concurrency,DocExtension,Unchecked,Arrays,Linq,Seq,Comparers,BaseComparer,BaseEqualityComparer,IntelliFactory,Runtime$1,Task,Handler,System,Guid,Client$1,Templates,DomUtility;
 Global=self;
 GlutenFree=Global.GlutenFree=Global.GlutenFree||{};
 OddJob=GlutenFree.OddJob=GlutenFree.OddJob||{};
 Manager=OddJob.Manager=OddJob.Manager||{};
 Presentation=Manager.Presentation=Manager.Presentation||{};
 WS=Presentation.WS=Presentation.WS||{};
 JSHelpers=WS.JSHelpers=WS.JSHelpers||{};
 ElementCreators=WS.ElementCreators=WS.ElementCreators||{};
 JobSearchClient=WS.JobSearchClient=WS.JobSearchClient||{};
 Client=WS.Client=WS.Client||{};
 WebSharper=Global.WebSharper;
 Obj=WebSharper&&WebSharper.Obj;
 JobSearchCriteria=WS.JobSearchCriteria=WS.JobSearchCriteria||{};
 UpdateForJob=WS.UpdateForJob=WS.UpdateForJob||{};
 UpdateForParam=WS.UpdateForParam=WS.UpdateForParam||{};
 JobRetryParameters=WS.JobRetryParameters=WS.JobRetryParameters||{};
 JobParameterDto=WS.JobParameterDto=WS.JobParameterDto||{};
 JobUpdateViewModel=WS.JobUpdateViewModel=WS.JobUpdateViewModel||{};
 JobMetadataResult=WS.JobMetadataResult=WS.JobMetadataResult||{};
 Template=WS.Template=WS.Template||{};
 Jobitem=Template.Jobitem=Template.Jobitem||{};
 Vars=Jobitem.Vars=Jobitem.Vars||{};
 UI=WebSharper&&WebSharper.UI;
 Templating=UI&&UI.Templating;
 Runtime=Templating&&Templating.Runtime;
 Server=Runtime&&Runtime.Server;
 TemplateInstance=Server&&Server.TemplateInstance;
 Instance=Jobitem.Instance=Jobitem.Instance||{};
 JobItem=Jobitem.JobItem=Jobitem.JobItem||{};
 Vars$1=JobItem.Vars=JobItem.Vars||{};
 Instance$1=JobItem.Instance=JobItem.Instance||{};
 Jobparameter=Template.Jobparameter=Template.Jobparameter||{};
 Vars$2=Jobparameter.Vars=Jobparameter.Vars||{};
 Instance$2=Jobparameter.Instance=Jobparameter.Instance||{};
 JobParameter=Jobparameter.JobParameter=Jobparameter.JobParameter||{};
 Vars$3=JobParameter.Vars=JobParameter.Vars||{};
 Instance$3=JobParameter.Instance=JobParameter.Instance||{};
 Jobsearch=Template.Jobsearch=Template.Jobsearch||{};
 Vars$4=Jobsearch.Vars=Jobsearch.Vars||{};
 Instance$4=Jobsearch.Instance=Jobsearch.Instance||{};
 ListItem=Jobsearch.ListItem=Jobsearch.ListItem||{};
 Vars$5=ListItem.Vars=ListItem.Vars||{};
 Instance$5=ListItem.Instance=ListItem.Instance||{};
 Main=Jobsearch.Main=Jobsearch.Main||{};
 Vars$6=Main.Vars=Main.Vars||{};
 Instance$6=Main.Instance=Main.Instance||{};
 Main$1=Template.Main=Template.Main||{};
 Vars$7=Main$1.Vars=Main$1.Vars||{};
 Instance$7=Main$1.Instance=Main$1.Instance||{};
 Editablejobitem=Template.Editablejobitem=Template.Editablejobitem||{};
 Vars$8=Editablejobitem.Vars=Editablejobitem.Vars||{};
 Instance$8=Editablejobitem.Instance=Editablejobitem.Instance||{};
 EditableJobItem=Editablejobitem.EditableJobItem=Editablejobitem.EditableJobItem||{};
 Vars$9=EditableJobItem.Vars=EditableJobItem.Vars||{};
 Instance$9=EditableJobItem.Instance=EditableJobItem.Instance||{};
 Searchoption=Template.Searchoption=Template.Searchoption||{};
 Vars$10=Searchoption.Vars=Searchoption.Vars||{};
 Instance$10=Searchoption.Instance=Searchoption.Instance||{};
 SearchOption=Searchoption.SearchOption=Searchoption.SearchOption||{};
 Vars$11=SearchOption.Vars=SearchOption.Vars||{};
 Instance$11=SearchOption.Instance=SearchOption.Instance||{};
 GlutenFree$OddJob$Manager$Presentation$WS_Templates=Global.GlutenFree$OddJob$Manager$Presentation$WS_Templates=Global.GlutenFree$OddJob$Manager$Presentation$WS_Templates||{};
 Strings=WebSharper&&WebSharper.Strings;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 View=UI&&UI.View;
 List=WebSharper&&WebSharper.List;
 AttrModule=UI&&UI.AttrModule;
 Var$1=UI&&UI.Var$1;
 Submitter=UI&&UI.Submitter;
 ListModel=UI&&UI.ListModel;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 DocExtension=UI&&UI.DocExtension;
 Unchecked=WebSharper&&WebSharper.Unchecked;
 Arrays=WebSharper&&WebSharper.Arrays;
 Linq=WebSharper&&WebSharper.Linq;
 Seq=WebSharper&&WebSharper.Seq;
 Comparers=WebSharper&&WebSharper.Comparers;
 BaseComparer=Comparers&&Comparers.BaseComparer;
 BaseEqualityComparer=Comparers&&Comparers.BaseEqualityComparer;
 IntelliFactory=Global.IntelliFactory;
 Runtime$1=IntelliFactory&&IntelliFactory.Runtime;
 Task=WebSharper&&WebSharper.Task;
 Handler=Server&&Server.Handler;
 System=Global.System;
 Guid=System&&System.Guid;
 Client$1=UI&&UI.Client;
 Templates=Client$1&&Client$1.Templates;
 DomUtility=UI&&UI.DomUtility;
 JSHelpers.TimeToString=function(time)
 {
  return(new Global.Date(time)).getHours()+":"+(new Global.Date(time)).getMinutes();
 };
 JSHelpers.DateToString=function(date)
 {
  return(new Global.Date(date)).getFullYear()+"-"+Strings.PadLeftWith(Global.String((new Global.Date(date)).getMonth()+1),2,"0")+"-"+Strings.PadLeftWith(Global.String((new Global.Date(date)).getDate()),2,"0");
 };
 ElementCreators.CheckableNumberInput=function(name,useInput,valueLens,defaultValue)
 {
  return Doc.ElementMixed("span",[Doc.CheckBox([],useInput),name,Doc.ElementMixed("input",[AttrProxy.Create("type","number"),valueLens,AttrProxy.Create("placeholder",Global.String(defaultValue))])]);
 };
 ElementCreators.CheckableTextInput=function(name,useInput,valueLens,defaultValue)
 {
  return Doc.ElementMixed("span",[Doc.CheckBox([],useInput),name,Doc.Input([AttrProxy.Create("placeholder",defaultValue)],valueLens)]);
 };
 ElementCreators.CheckableTextInput$1=function(name,useInput,valueLens)
 {
  return Doc.ElementMixed("span",[Doc.CheckBox([],useInput),name,Doc.Input([],valueLens)]);
 };
 ElementCreators.ClearableTimeInput=function(timeLens)
 {
  function callback()
  {
   var $1;
   timeLens.Set($1="");
   return $1;
  }
  return Doc.ElementMixed("span",[Doc.Input([AttrProxy.Create("type","time")],timeLens),Doc.Button("Clear",[],function()
  {
   callback();
  })]);
 };
 ElementCreators.ClearableDateInput=function(dateLens)
 {
  function callback()
  {
   var $1;
   dateLens.Set($1="");
   return $1;
  }
  return Doc.ElementMixed("span",[Doc.Input([AttrProxy.Create("type","date")],dateLens),Doc.Button("Clear",[],function()
  {
   callback();
  })]);
 };
 ElementCreators.DateTimeRangeSearch=function(name,useCriteriaLens,beforeDateLens,beforeTimeLens,afterDateLens,afterTimeLens)
 {
  return Doc.ElementMixed("span",[Doc.CheckBox([],useCriteriaLens),name+": ",Doc.ElementMixed("br",[]),Doc.ElementMixed("span",[ElementCreators.ClearableDateInput(beforeDateLens),ElementCreators.ClearableTimeInput(beforeTimeLens)]),Doc.ElementMixed("span",[ElementCreators.ClearableDateInput(afterDateLens),ElementCreators.ClearableTimeInput(afterTimeLens)])]);
 };
 ElementCreators.DateRangeSearch=function(name,useCriteriaLens,beforeLens,afterLens)
 {
  return Doc.ElementMixed("span",[Doc.CheckBox([],useCriteriaLens),name+": ",Doc.Input([AttrProxy.Create("type","date")],beforeLens),Doc.Input([AttrProxy.Create("type","date")],afterLens)]);
 };
 ElementCreators.OptionSearch=function(name,criteriaLens,optionView,useCriteriaLens,changeAction)
 {
  var del;
  return Doc.ElementMixed("span",[Doc.CheckBox([],useCriteriaLens),name+": ",Doc.SelectDyn([],function(q)
  {
   return q===null?"Please Select a "+name+"...":q;
  },View.Map(List.ofSeq,optionView),criteriaLens),AttrModule.Handler("change",(del=changeAction,function(a)
  {
   return function(b)
   {
    return del(a,b);
   };
  }))]);
 };
 ElementCreators.TextSearch=function(name,criteriaLens,useCriteriaLens)
 {
  return Doc.ElementMixed("span",[Doc.CheckBox([],useCriteriaLens),name,Doc.Input([],criteriaLens)]);
 };
 JobSearchClient.Main=function()
 {
  var criteria,a,useQueueLens,a$1,statusLens,a$2,useStatus,a$3,methodLens,a$4,useMethod,statusOptions,dummyQueueCriteriaFiller,criteriaFiller,updateSet,queueNames,queueNameView,methodCriteria,submit,result,results,a$5,queueNameLens,a$6,useCreatedLens,a$7,createdBeforeDateLens,a$8,createdAfterDateLens,a$9,createdBeforeTimeLens,a$10,createdAfterTimeLens,a$11,useAttemptedDTLens,a$12,lastExecutedBeforeTimeLens,a$13,lastExecutedBeforeDateLens,a$14,lastExecutedAfterTimeLens,a$15,lastExecutedAfterDateLens,callback,content;
  criteria=Var$1.Create$1(new JobSearchCriteria.New());
  function del(a$16,b)
  {
   a$16.UseQueue=b;
   return a$16;
  }
  useQueueLens=(a=function(a$16)
  {
   return function(b)
   {
    return del(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.UseQueue;
  },function($1,$2)
  {
   return(a($1))($2);
  }));
  function del$1(a$16,b)
  {
   a$16.set_Status(b);
   return a$16;
  }
  statusLens=(a$1=function(a$16)
  {
   return function(b)
   {
    return del$1(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.get_Status();
  },function($1,$2)
  {
   return(a$1($1))($2);
  }));
  function del$2(a$16,b)
  {
   a$16.UseStatus=b;
   return a$16;
  }
  useStatus=(a$2=function(a$16)
  {
   return function(b)
   {
    return del$2(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.UseStatus;
  },function($1,$2)
  {
   return(a$2($1))($2);
  }));
  function del$3(a$16,b)
  {
   a$16.set_MethodName(b);
   return a$16;
  }
  methodLens=(a$3=function(a$16)
  {
   return function(b)
   {
    return del$3(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.get_MethodName();
  },function($1,$2)
  {
   return(a$3($1))($2);
  }));
  function del$4(a$16,b)
  {
   a$16.UseMethod=b;
   return a$16;
  }
  useMethod=(a$4=function(a$16)
  {
   return function(b)
   {
    return del$4(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.UseMethod;
  },function($1,$2)
  {
   return(a$4($1))($2);
  }));
  statusOptions=Var$1.Create$1([null,"Processed","New","Failed","Retry","InProgress","Inserting"]);
  dummyQueueCriteriaFiller=Var$1.Create$1("");
  criteriaFiller=Submitter.CreateOption(dummyQueueCriteriaFiller.get_View());
  updateSet=new ListModel.New$1(function(q)
  {
   return q.JobGuid;
  });
  queueNames=Var$1.Create$1([null]);
  function f(input)
  {
   var $task,$run,$state,$1,$2,$3,$4,$await;
   $task=new WebSharper.Task1({
    status:3,
    continuations:[]
   });
   $state=0;
   $run=function()
   {
    $top:while(true)
     switch($state)
     {
      case 0:
       $3=void 0;
       $4=void 0;
       $3=void 0;
       $4=void 0;
       $await=void 0;
       $3=queueNames;
       $await=(new AjaxRemotingProvider.New()).Task("GlutenFree.OddJob.Manager.Presentation.WS:GlutenFree.OddJob.Manager.Presentation.WS.Remoting.GetQueueNameList:1051861547",[]);
       $state=1;
       $await.OnCompleted($run);
       return;
      case 1:
       if($await.exc)
        {
         $task.exc=$await.exc;
         $task.status=7;
         $task.RunContinuations();
         return;
        }
       $4=$await.result;
       $3.Set($4);
       $task.result=queueNames.Get();
       $task.status=5;
       $task.RunContinuations();
       return;
     }
   };
   $run();
   return $task;
  }
  queueNameView=View.MapAsync(function(a$16)
  {
   return Concurrency.AwaitTask1(f(a$16));
  },criteriaFiller.view);
  criteriaFiller.Trigger();
  methodCriteria=Var$1.Create$1([null]);
  submit=Submitter.CreateOption(criteria.get_View());
  result=DocExtension.Doc$1(updateSet,function(juvm)
  {
   var map,updateSubmitter,updateResult,jobParamUpdate,a$16,newMethodName,a$17,updateMethodName,a$18,updateStatus,a$19,newStatus,a$20,updateQueue,a$21,newQueue,a$22,updateMaxRetryCount,a$23,newMaxRetryCount;
   map=updateSet.Lens(juvm.JobGuid);
   updateSubmitter=Submitter.CreateOption(map.get_View());
   function f$2(input)
   {
    var $task,$run,$state,$await,success;
    $task=new WebSharper.Task1({
     status:3,
     continuations:[]
    });
    $state=0;
    $run=function()
    {
     $top:while(true)
      switch($state)
      {
       case 0:
        if(Unchecked.Equals(input,null))
         {
          $task.result=Doc.ElementMixed("div",[""]);
          $task.status=5;
          $task.RunContinuations();
          return;
         }
        $await=void 0;
        $await=(new AjaxRemotingProvider.New()).Task("GlutenFree.OddJob.Manager.Presentation.WS:GlutenFree.OddJob.Manager.Presentation.WS.Remoting.UpdateJob:-1756354977",[input.$0]);
        $state=1;
        $await.OnCompleted($run);
        return;
       case 1:
        if($await.exc)
         {
          $task.exc=$await.exc;
          $task.status=7;
          $task.RunContinuations();
          return;
         }
        success=$await.result;
        $task.result=Doc.ElementMixed("div",[success?"Updated":"Failed update"]);
        $task.status=5;
        $task.RunContinuations();
        return;
      }
    };
    $run();
    return $task;
   }
   updateResult=View.MapAsync(function(a$24)
   {
    return Concurrency.AwaitTask1(f$2(a$24));
   },updateSubmitter.view);
   jobParamUpdate=Arrays.ofSeq(Linq.Select(juvm.UpdateDate.ParamUpdates,function(updateDateParamUpdate,i)
   {
    var a$24,updateParamTypeLens,a$25,newParamTypeLens,a$26,updateParamValueLens,a$27,newParamValueLens;
    function del$24(a$28,b)
    {
     a$28.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).UpdateParamType=b;
     return a$28;
    }
    updateParamTypeLens=(a$24=function(a$28)
    {
     return function(b)
     {
      return del$24(a$28,b);
     };
    },Var$1.Lens(map,function(uvm)
    {
     return uvm.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).UpdateParamType;
    },function($1,$2)
    {
     return(a$24($1))($2);
    }));
    function del$25(a$28,b)
    {
     a$28.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).NewParamType=b;
     return a$28;
    }
    newParamTypeLens=(a$25=function(a$28)
    {
     return function(b)
     {
      return del$25(a$28,b);
     };
    },Var$1.Lens(map,function(uvm)
    {
     return uvm.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).NewParamType;
    },function($1,$2)
    {
     return(a$25($1))($2);
    }));
    function del$26(a$28,b)
    {
     a$28.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).UpdateParamValue=b;
     return a$28;
    }
    updateParamValueLens=(a$26=function(a$28)
    {
     return function(b)
     {
      return del$26(a$28,b);
     };
    },Var$1.Lens(map,function(uvm)
    {
     return uvm.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).UpdateParamValue;
    },function($1,$2)
    {
     return(a$26($1))($2);
    }));
    function del$27(a$28,b)
    {
     a$28.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).NewParamValue=b;
     return a$28;
    }
    newParamValueLens=(a$27=function(a$28)
    {
     return function(b)
     {
      return del$27(a$28,b);
     };
    },Var$1.Lens(map,function(uvm)
    {
     return uvm.UpdateDate.ParamUpdates.get_Item(updateDateParamUpdate.K).NewParamValue;
    },function($1,$2)
    {
     return(a$27($1))($2);
    }));
    return Doc.ElementMixed("div",[ElementCreators.CheckableTextInput("Type",updateParamTypeLens,newParamTypeLens,juvm.MetaData.JobArgs[i].Type),ElementCreators.CheckableTextInput("Value",updateParamValueLens,newParamValueLens,juvm.MetaData.JobArgs[i].Value)]);
   }));
   function del$16(a$24,b)
   {
    a$24.UpdateDate.NewMethodName=b;
    return a$24;
   }
   newMethodName=(a$16=function(a$24)
   {
    return function(b)
    {
     return del$16(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.NewMethodName;
   },function($1,$2)
   {
    return(a$16($1))($2);
   }));
   function del$17(a$24,b)
   {
    a$24.UpdateDate.UpdateMethodName=b;
    return a$24;
   }
   updateMethodName=(a$17=function(a$24)
   {
    return function(b)
    {
     return del$17(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.UpdateMethodName;
   },function($1,$2)
   {
    return(a$17($1))($2);
   }));
   function del$18(a$24,b)
   {
    a$24.UpdateDate.UpdateStatus=b;
    return a$24;
   }
   updateStatus=(a$18=function(a$24)
   {
    return function(b)
    {
     return del$18(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.UpdateStatus;
   },function($1,$2)
   {
    return(a$18($1))($2);
   }));
   function del$19(a$24,b)
   {
    a$24.UpdateDate.NewStatus=b;
    return a$24;
   }
   newStatus=(a$19=function(a$24)
   {
    return function(b)
    {
     return del$19(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.NewStatus;
   },function($1,$2)
   {
    return(a$19($1))($2);
   }));
   function del$20(a$24,b)
   {
    a$24.UpdateDate.UpdateQueueName=b;
    return a$24;
   }
   updateQueue=(a$20=function(a$24)
   {
    return function(b)
    {
     return del$20(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.UpdateQueueName;
   },function($1,$2)
   {
    return(a$20($1))($2);
   }));
   function del$21(a$24,b)
   {
    a$24.UpdateDate.NewQueueName=b;
    return a$24;
   }
   newQueue=(a$21=function(a$24)
   {
    return function(b)
    {
     return del$21(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.NewQueueName;
   },function($1,$2)
   {
    return(a$21($1))($2);
   }));
   function del$22(a$24,b)
   {
    a$24.UpdateDate.UpdateRetryCount=b;
    return a$24;
   }
   updateMaxRetryCount=(a$22=function(a$24)
   {
    return function(b)
    {
     return del$22(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.UpdateRetryCount;
   },function($1,$2)
   {
    return(a$22($1))($2);
   }));
   function del$23(a$24,b)
   {
    a$24.UpdateDate.NewMaxRetryCount=b;
    return a$24;
   }
   newMaxRetryCount=(a$23=function(a$24)
   {
    return function(b)
    {
     return del$23(a$24,b);
    };
   },Var$1.Lens(map,function(uvm)
   {
    return uvm.UpdateDate.NewMaxRetryCount;
   },function($1,$2)
   {
    return(a$23($1))($2);
   }));
   function callback$1()
   {
    updateSubmitter.Trigger();
   }
   return Doc.ElementMixed("div",[AttrModule.Style("display","grid"),AttrModule.Style("grid-template-columns","50% 50%"),Doc.ElementMixed("div",[(new JobItem.New()).MethodName$1(juvm.MetaData.MethodName).QueueName$1(juvm.MetaData.Queue).Status$1(juvm.MetaData.Status).JobGuid$1(juvm.MetaData.JobId).Doc()]),Doc.ElementMixed("div",[Doc.ElementMixed("div",[Doc.ElementMixed("div",[ElementCreators.CheckableTextInput("Status",updateStatus,newStatus,juvm.MetaData.Status)]),Doc.ElementMixed("div",[ElementCreators.CheckableTextInput("MethodName",updateMethodName,newMethodName,juvm.MetaData.MethodName)]),Doc.ElementMixed("div",[ElementCreators.CheckableTextInput("QueueName",updateQueue,newQueue,juvm.MetaData.Queue)]),Doc.ElementMixed("div",[ElementCreators.CheckableNumberInput("MaxRetryCount",updateMaxRetryCount,newMaxRetryCount,juvm.MetaData.RetryParameters.MaxRetries)])])]),Doc.ElementMixed("div",[Doc.ElementMixed("div",Arrays.ofSeq(Linq.Select(juvm.MetaData.JobArgs,function(r,i)
   {
    return Doc.ElementMixed("span",[(new JobParameter.New()).Type$1(r.Type).Name$1(r.Name).Value$1(r.Value).Ordinal$1(Global.String(i)).Doc()]);
   })))]),Doc.ElementMixed("div",jobParamUpdate),Doc.Button("Update",[],function()
   {
    callback$1();
   }),Doc.ElementMixed("div",[updateResult])]);
  });
  function f$1(input)
  {
   var $task,$run,$state,methodOptionFuture,$await,awaitedMethodOptions,future,$await$1,awaitedFuture;
   $task=new WebSharper.Task1({
    status:3,
    continuations:[]
   });
   $state=0;
   $run=function()
   {
    $top:while(true)
     switch($state)
     {
      case 0:
       if(Unchecked.Equals(input,null))
        {
         $task.result=Doc.ElementMixed("div",[""]);
         $task.status=5;
         $task.RunContinuations();
         return;
        }
       methodOptionFuture=(new AjaxRemotingProvider.New()).Task("GlutenFree.OddJob.Manager.Presentation.WS:GlutenFree.OddJob.Manager.Presentation.WS.Remoting.GetMethods:1236116521",[input.$0.get_QueueName()]);
       $await=void 0;
       $await=methodOptionFuture;
       $state=1;
       $await.OnCompleted($run);
       return;
      case 1:
       if($await.exc)
        {
         $task.exc=$await.exc;
         $task.status=7;
         $task.RunContinuations();
         return;
        }
       awaitedMethodOptions=$await.result;
       methodCriteria.Set(awaitedMethodOptions);
       future=(new AjaxRemotingProvider.New()).Task("GlutenFree.OddJob.Manager.Presentation.WS:GlutenFree.OddJob.Manager.Presentation.WS.Remoting.SearchCriteria:306186415",[input.$0]);
       $await$1=void 0;
       $await$1=future;
       $state=2;
       $await$1.OnCompleted($run);
       return;
      case 2:
       if($await$1.exc)
        {
         $task.exc=$await$1.exc;
         $task.status=7;
         $task.RunContinuations();
         return;
        }
       awaitedFuture=$await$1.result;
       updateSet.Set(Seq.map(function(q)
       {
        var $1,$2;
        $1=new JobUpdateViewModel.New();
        $1.JobGuid=q.JobId;
        $1.MetaData=q;
        $1.UpdateDate=($2=new UpdateForJob.New(),$2.JobGuid=q.JobId,$2.OldStatus=q.Status,$2.ParamUpdates=Linq.ToDictionary(Linq.Select(Linq.OrderBy(q.JobArgs,function(a$16)
        {
         return a$16.Ordinal;
        },new BaseComparer.New()),function(r,i)
        {
         return{
          key:Global.String(i),
          value:new UpdateForParam.New()
         };
        }),function(r)
        {
         return r.key;
        },function(s)
        {
         return s.value;
        },new BaseEqualityComparer.New()),$2);
        return $1;
       },awaitedFuture));
       $task.result=Doc.ElementMixed("div",[Doc.ElementMixed("h3",["Results:"]),Doc.ElementMixed("br",[]),Doc.ConcatMixed([result])]);
       $task.status=5;
       $task.RunContinuations();
       return;
     }
   };
   $run();
   return $task;
  }
  results=View.MapAsync(function(a$16)
  {
   return Concurrency.AwaitTask1(f$1(a$16));
  },submit.view);
  function del$5(a$16,b)
  {
   a$16.set_QueueName(b);
   return a$16;
  }
  queueNameLens=(a$5=function(a$16)
  {
   return function(b)
   {
    return del$5(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.get_QueueName();
  },function($1,$2)
  {
   return(a$5($1))($2);
  }));
  function del$6(a$16,b)
  {
   a$16.useCreatedDate=b;
   return a$16;
  }
  useCreatedLens=(a$6=function(a$16)
  {
   return function(b)
   {
    return del$6(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.useCreatedDate;
  },function($1,$2)
  {
   return(a$6($1))($2);
  }));
  function del$7(a$16,b)
  {
   a$16.createdBefore=b;
   return a$16;
  }
  createdBeforeDateLens=(a$7=function(a$16)
  {
   return function(b)
   {
    return del$7(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.createdBefore;
  },function($1,$2)
  {
   return(a$7($1))($2);
  }));
  function del$8(a$16,b)
  {
   a$16.createdBefore=b;
   return a$16;
  }
  createdAfterDateLens=(a$8=function(a$16)
  {
   return function(b)
   {
    return del$8(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.createdAfter;
  },function($1,$2)
  {
   return(a$8($1))($2);
  }));
  function del$9(a$16,b)
  {
   a$16.createdBeforeTime=b;
   return a$16;
  }
  createdBeforeTimeLens=(a$9=function(a$16)
  {
   return function(b)
   {
    return del$9(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.createdBeforeTime;
  },function($1,$2)
  {
   return(a$9($1))($2);
  }));
  function del$10(a$16,b)
  {
   a$16.createdAfterTime=b;
   return a$16;
  }
  createdAfterTimeLens=(a$10=function(a$16)
  {
   return function(b)
   {
    return del$10(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.createdAfterTime;
  },function($1,$2)
  {
   return(a$10($1))($2);
  }));
  function del$11(a$16,b)
  {
   a$16.useLastAttemptDate=b;
   return a$16;
  }
  useAttemptedDTLens=(a$11=function(a$16)
  {
   return function(b)
   {
    return del$11(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.useLastAttemptDate;
  },function($1,$2)
  {
   return(a$11($1))($2);
  }));
  function del$12(a$16,b)
  {
   a$16.attemptedBeforeTime=b;
   return a$16;
  }
  lastExecutedBeforeTimeLens=(a$12=function(a$16)
  {
   return function(b)
   {
    return del$12(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.attemptedBeforeTime;
  },function($1,$2)
  {
   return(a$12($1))($2);
  }));
  function del$13(a$16,b)
  {
   a$16.attemptedBeforeDate=b;
   return a$16;
  }
  lastExecutedBeforeDateLens=(a$13=function(a$16)
  {
   return function(b)
   {
    return del$13(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.attemptedBeforeDate;
  },function($1,$2)
  {
   return(a$13($1))($2);
  }));
  function del$14(a$16,b)
  {
   a$16.attemptedAfterTime=b;
   return a$16;
  }
  lastExecutedAfterTimeLens=(a$14=function(a$16)
  {
   return function(b)
   {
    return del$14(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.attemptedAfterTime;
  },function($1,$2)
  {
   return(a$14($1))($2);
  }));
  function del$15(a$16,b)
  {
   a$16.attemptedAfterDate=b;
   return a$16;
  }
  lastExecutedAfterDateLens=(a$15=function(a$16)
  {
   return function(b)
   {
    return del$15(a$16,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.attemptedAfterDate;
  },function($1,$2)
  {
   return(a$15($1))($2);
  }));
  content=Doc.ElementMixed("div",[Doc.ElementMixed("div",[AttrModule.Style("width","100%"),Doc.ElementMixed("div",[ElementCreators.OptionSearch("Queue Name",queueNameLens,queueNameView,useQueueLens,function()
  {
   return submit.Trigger();
  }),ElementCreators.TextSearch("Method Name",methodLens,useMethod),ElementCreators.OptionSearch("Status",statusLens,statusOptions.get_View(),useStatus,function()
  {
   return submit.Trigger();
  }),ElementCreators.OptionSearch("Method",methodLens,methodCriteria.get_View(),useMethod,function()
  {
   return submit.Trigger();
  }),ElementCreators.DateTimeRangeSearch("Created",useCreatedLens,createdBeforeDateLens,createdBeforeTimeLens,createdAfterDateLens,createdAfterTimeLens),ElementCreators.DateTimeRangeSearch("Attempt",useAttemptedDTLens,lastExecutedBeforeDateLens,lastExecutedBeforeTimeLens,lastExecutedAfterDateLens,lastExecutedAfterTimeLens),Doc.ElementMixed("div",[(callback=Runtime$1.BindDelegate(Submitter.prototype.Trigger,submit),Doc.Button("Search",[],function()
  {
   callback();
  }))])])]),Doc.ElementMixed("div",[Doc.ElementMixed("br",[])]),Doc.ElementMixed("div",[results])]);
  return content;
 };
 JobSearchClient.BuildUpdateForJob=function(juvm)
 {
  return Doc.ElementMixed("div",[]);
 };
 Client.Main=function()
 {
  var rvInput,submit,vReversed,callback;
  rvInput=Var$1.Create$1(new JobSearchCriteria.New());
  submit=Submitter.CreateOption(rvInput.get_View());
  function f(input)
  {
   if(Unchecked.Equals(input,null))
    return Task.FromResult("");
   return(new AjaxRemotingProvider.New()).Task("GlutenFree.OddJob.Manager.Presentation.WS:GlutenFree.OddJob.Manager.Presentation.WS.Remoting.DoSomething:1142547252",[input.$0]);
  }
  vReversed=View.MapAsync(function(a)
  {
   return Concurrency.AwaitTask1(f(a));
  },submit.view);
  return Doc.ElementMixed("div",[Doc.ElementMixed("input",[rvInput]),(callback=Runtime$1.BindDelegate(Submitter.prototype.Trigger,submit),Doc.Button("Send",[],function()
  {
   callback();
  })),Doc.ElementMixed("hr",[]),Doc.ElementMixed("h4",[AttrProxy.Create("class","text-muted"),"The server responded:",Doc.ElementMixed("div",[AttrProxy.Create("class","jumbotron"),Doc.ElementMixed("h1",[vReversed])])])]);
 };
 JobSearchCriteria=WS.JobSearchCriteria=Runtime$1.Class({
  set_JobGuid:function(value)
  {
   this.$JobGuid=value;
  },
  get_JobGuid:function()
  {
   return this.$JobGuid;
  },
  set_Status:function(value)
  {
   this.$Status=value;
  },
  get_Status:function()
  {
   return this.$Status;
  },
  set_MethodName:function(value)
  {
   this.$MethodName=value;
  },
  get_MethodName:function()
  {
   return this.$MethodName;
  },
  set_QueueName:function(value)
  {
   this.$QueueName=value;
  },
  get_QueueName:function()
  {
   return this.$QueueName;
  },
  $init:function()
  {
   this.UseMethod=null;
   this.UseStatus=null;
   this.useCreatedDate=null;
   this.useLastAttemptDate=null;
   this.createdBefore="";
   this.createdAfter="";
   this.attemptedBeforeDate="";
   this.attemptedAfterDate="";
   this.attemptedBeforeTime="";
   this.attemptedAfterTime="";
   this.createdBeforeTime="";
   this.createdAfterTime="";
   this.UseQueue=true;
   this.set_QueueName(null);
   this.set_MethodName(null);
   this.set_Status(null);
   this.set_JobGuid(null);
  }
 },Obj,JobSearchCriteria);
 JobSearchCriteria.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobSearchCriteria);
 UpdateForJob=WS.UpdateForJob=Runtime$1.Class({
  $init:function()
  {
   this.JobGuid=null;
   this.OldStatus=null;
   this.UpdateRetryCount=null;
   this.NewMaxRetryCount=0;
   this.RequireOldStatus=null;
   this.UpdateMethodName=null;
   this.NewMethodName=null;
   this.UpdateQueueName=null;
   this.NewQueueName=null;
   this.UpdateStatus=null;
   this.NewStatus=null;
   this.ParamUpdates=null;
  }
 },Obj,UpdateForJob);
 UpdateForJob.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },UpdateForJob);
 UpdateForParam=WS.UpdateForParam=Runtime$1.Class({
  $init:function()
  {
   this.UpdateParamType=null;
   this.NewParamType=null;
   this.UpdateParamValue=null;
   this.NewParamValue=null;
  }
 },Obj,UpdateForParam);
 UpdateForParam.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },UpdateForParam);
 JobRetryParameters=WS.JobRetryParameters=Runtime$1.Class({
  $init:function()
  {
   this.MaxRetries=0;
   this.MinRetryWait=0;
   this.RetryCount=0;
   this.LastAttempt=null;
  }
 },Obj,JobRetryParameters);
 JobRetryParameters.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobRetryParameters);
 JobParameterDto=WS.JobParameterDto=Runtime$1.Class({
  $init:function()
  {
   this.Ordinal=0;
   this.Type=null;
   this.Name=null;
   this.Value=null;
  }
 },Obj,JobParameterDto);
 JobParameterDto.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobParameterDto);
 JobUpdateViewModel=WS.JobUpdateViewModel=Runtime$1.Class({
  $init:function()
  {
   this.JobGuid=null;
   this.UpdateDate=null;
   this.MetaData=null;
  }
 },Obj,JobUpdateViewModel);
 JobUpdateViewModel.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobUpdateViewModel);
 JobMetadataResult=WS.JobMetadataResult=Runtime$1.Class({
  $init:function()
  {
   this.JobId=null;
   this.JobArgs=null;
   this.TypeExecutedOn=null;
   this.MethodName=null;
   this.Status=null;
   this.MethodGenericTypes=null;
   this.ExecutionTime=null;
   this.RetryParameters=null;
   this.Queue=null;
  }
 },Obj,JobMetadataResult);
 JobMetadataResult.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobMetadataResult);
 Jobitem=Template.Jobitem=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.t(completed[0]);
   this.instance=new Instance.New(completed[1],doc);
   return this.instance;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,Jobitem);
 Jobitem.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Jobitem);
 Vars=Jobitem.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars);
 Vars.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars);
 Vars.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars);
 Instance=Jobitem.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars.New$1(this);
  }
 },TemplateInstance,Instance);
 Instance.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance);
 JobItem=Jobitem.JobItem=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.jobitem(completed[0]);
   this.instance=new Instance$1.New(completed[1],doc);
   return this.instance;
  },
  Status:function(x)
  {
   this.holes.push({
    $:2,
    $0:"status",
    $1:x
   });
   return this;
  },
  Status$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"status",
    $1:x
   });
   return this;
  },
  QueueName:function(x)
  {
   this.holes.push({
    $:2,
    $0:"queuename",
    $1:x
   });
   return this;
  },
  QueueName$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"queuename",
    $1:x
   });
   return this;
  },
  MethodName:function(x)
  {
   this.holes.push({
    $:2,
    $0:"methodname",
    $1:x
   });
   return this;
  },
  MethodName$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"methodname",
    $1:x
   });
   return this;
  },
  JobGuid:function(x)
  {
   this.holes.push({
    $:2,
    $0:"jobguid",
    $1:x
   });
   return this;
  },
  JobGuid$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"jobguid",
    $1:x
   });
   return this;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,JobItem);
 JobItem.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobItem);
 Vars$1=JobItem.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$1);
 Vars$1.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$1);
 Vars$1.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$1);
 Instance$1=JobItem.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$1.New$1(this);
  }
 },TemplateInstance,Instance$1);
 Instance$1.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$1);
 Jobparameter=Template.Jobparameter=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.t(completed[0]);
   this.instance=new Instance$2.New(completed[1],doc);
   return this.instance;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,Jobparameter);
 Jobparameter.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Jobparameter);
 Vars$2=Jobparameter.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$2);
 Vars$2.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$2);
 Vars$2.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$2);
 Instance$2=Jobparameter.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$2.New$1(this);
  }
 },TemplateInstance,Instance$2);
 Instance$2.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$2);
 JobParameter=Jobparameter.JobParameter=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.jobparameter(completed[0]);
   this.instance=new Instance$3.New(completed[1],doc);
   return this.instance;
  },
  Value:function(x)
  {
   this.holes.push({
    $:2,
    $0:"value",
    $1:x
   });
   return this;
  },
  Value$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"value",
    $1:x
   });
   return this;
  },
  Type:function(x)
  {
   this.holes.push({
    $:2,
    $0:"type",
    $1:x
   });
   return this;
  },
  Type$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"type",
    $1:x
   });
   return this;
  },
  Name:function(x)
  {
   this.holes.push({
    $:2,
    $0:"name",
    $1:x
   });
   return this;
  },
  Name$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"name",
    $1:x
   });
   return this;
  },
  Ordinal:function(x)
  {
   this.holes.push({
    $:2,
    $0:"ordinal",
    $1:x
   });
   return this;
  },
  Ordinal$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"ordinal",
    $1:x
   });
   return this;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,JobParameter);
 JobParameter.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobParameter);
 Vars$3=JobParameter.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$3);
 Vars$3.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$3);
 Vars$3.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$3);
 Instance$3=JobParameter.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$3.New$1(this);
  }
 },TemplateInstance,Instance$3);
 Instance$3.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$3);
 Jobsearch=Template.Jobsearch=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.t$1(completed[0]);
   this.instance=new Instance$4.New(completed[1],doc);
   return this.instance;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,Jobsearch);
 Jobsearch.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Jobsearch);
 Vars$4=Jobsearch.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$4);
 Vars$4.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$4);
 Vars$4.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$4);
 Instance$4=Jobsearch.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$4.New$1(this);
  }
 },TemplateInstance,Instance$4);
 Instance$4.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$4);
 ListItem=Jobsearch.ListItem=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.listitem(completed[0]);
   this.instance=new Instance$5.New(completed[1],doc);
   return this.instance;
  },
  Job:function(x)
  {
   this.holes.push({
    $:2,
    $0:"job",
    $1:x
   });
   return this;
  },
  Job$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"job",
    $1:x
   });
   return this;
  },
  ShowDone:function(x)
  {
   x=x==void 0?[]:x;
   this.holes.push({
    $:3,
    $0:"showdone",
    $1:AttrProxy.Concat(x)
   });
   return this;
  },
  ShowDone$1:function(x)
  {
   this.holes.push({
    $:3,
    $0:"showdone",
    $1:AttrProxy.Concat(x)
   });
   return this;
  },
  ShowDone$2:function(x)
  {
   this.holes.push({
    $:3,
    $0:"showdone",
    $1:x
   });
   return this;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,ListItem);
 ListItem.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },ListItem);
 Vars$5=ListItem.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$5);
 Vars$5.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$5);
 Vars$5.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$5);
 Instance$5=ListItem.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$5.New$1(this);
  }
 },TemplateInstance,Instance$5);
 Instance$5.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$5);
 Main=Jobsearch.Main=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[["searchqueuename",0],["searchmethodname",0]]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.main(completed[0]);
   this.instance=new Instance$6.New(completed[1],doc);
   return this.instance;
  },
  Clear:function(x)
  {
   var $this;
   $this=this;
   function del(a,b)
   {
    return x({
     Vars:new Vars$6.New$1($this.instance),
     Target:a,
     Event:b
    });
   }
   this.holes.push({
    $:4,
    $0:"clear",
    $1:function(a)
    {
     return function(b)
     {
      return del(a,b);
     };
    }
   });
   return this;
  },
  Clear$1:function(x)
  {
   function del(a,b)
   {
    return x();
   }
   this.holes.push({
    $:4,
    $0:"clear",
    $1:function(a)
    {
     return function(b)
     {
      return del(a,b);
     };
    }
   });
   return this;
  },
  Clear$2:function(x)
  {
   var f;
   this.holes.push((f=x,{
    $:4,
    $0:"clear",
    $1:function(el)
    {
     return function(ev)
     {
      return f(el,ev);
     };
    }
   }));
   return this;
  },
  NewTaskName:function(x)
  {
   this.holes.push({
    $:2,
    $0:"newtaskname",
    $1:x
   });
   return this;
  },
  NewTaskName$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"newtaskname",
    $1:x
   });
   return this;
  },
  Search:function(x)
  {
   var $this;
   $this=this;
   function del(a,b)
   {
    return x({
     Vars:new Vars$6.New$1($this.instance),
     Target:a,
     Event:b
    });
   }
   this.holes.push({
    $:4,
    $0:"search",
    $1:function(a)
    {
     return function(b)
     {
      return del(a,b);
     };
    }
   });
   return this;
  },
  Search$1:function(x)
  {
   function del(a,b)
   {
    return x();
   }
   this.holes.push({
    $:4,
    $0:"search",
    $1:function(a)
    {
     return function(b)
     {
      return del(a,b);
     };
    }
   });
   return this;
  },
  Search$2:function(x)
  {
   var f;
   this.holes.push((f=x,{
    $:4,
    $0:"search",
    $1:function(el)
    {
     return function(ev)
     {
      return f(el,ev);
     };
    }
   }));
   return this;
  },
  SearchMethodName:function(x)
  {
   this.holes.push({
    $:8,
    $0:"searchmethodname",
    $1:x
   });
   return this;
  },
  SearchQueueName:function(x)
  {
   this.holes.push({
    $:8,
    $0:"searchqueuename",
    $1:x
   });
   return this;
  },
  ListContainer:function(x)
  {
   this.holes.push({
    $:2,
    $0:"listcontainer",
    $1:x
   });
   return this;
  },
  ListContainer$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"listcontainer",
    $1:x
   });
   return this;
  },
  ListContainer$2:function(x)
  {
   x=x==void 0?[]:x;
   this.holes.push({
    $:0,
    $0:"listcontainer",
    $1:Doc.Concat(x)
   });
   return this;
  },
  ListContainer$3:function(x)
  {
   this.holes.push({
    $:0,
    $0:"listcontainer",
    $1:Doc.Concat(x)
   });
   return this;
  },
  ListContainer$4:function(x)
  {
   this.holes.push({
    $:0,
    $0:"listcontainer",
    $1:x
   });
   return this;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,Main);
 Main.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Main);
 Vars$6=Main.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$6);
 Vars$6.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$6);
 Vars$6.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$6);
 Instance$6=Main.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$6.New$1(this);
  }
 },TemplateInstance,Instance$6);
 Instance$6.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$6);
 Main$1=Template.Main=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.t$2(completed[0]);
   this.instance=new Instance$7.New(completed[1],doc);
   return this.instance;
  },
  scripts:function(x)
  {
   this.holes.push({
    $:2,
    $0:"scripts",
    $1:x
   });
   return this;
  },
  scripts$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"scripts",
    $1:x
   });
   return this;
  },
  scripts$2:function(x)
  {
   x=x==void 0?[]:x;
   this.holes.push({
    $:0,
    $0:"scripts",
    $1:Doc.Concat(x)
   });
   return this;
  },
  scripts$3:function(x)
  {
   this.holes.push({
    $:0,
    $0:"scripts",
    $1:Doc.Concat(x)
   });
   return this;
  },
  scripts$4:function(x)
  {
   this.holes.push({
    $:0,
    $0:"scripts",
    $1:x
   });
   return this;
  },
  Body:function(x)
  {
   this.holes.push({
    $:2,
    $0:"body",
    $1:x
   });
   return this;
  },
  Body$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"body",
    $1:x
   });
   return this;
  },
  Body$2:function(x)
  {
   x=x==void 0?[]:x;
   this.holes.push({
    $:0,
    $0:"body",
    $1:Doc.Concat(x)
   });
   return this;
  },
  Body$3:function(x)
  {
   this.holes.push({
    $:0,
    $0:"body",
    $1:Doc.Concat(x)
   });
   return this;
  },
  Body$4:function(x)
  {
   this.holes.push({
    $:0,
    $0:"body",
    $1:x
   });
   return this;
  },
  MenuBar:function(x)
  {
   this.holes.push({
    $:2,
    $0:"menubar",
    $1:x
   });
   return this;
  },
  MenuBar$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"menubar",
    $1:x
   });
   return this;
  },
  MenuBar$2:function(x)
  {
   x=x==void 0?[]:x;
   this.holes.push({
    $:0,
    $0:"menubar",
    $1:Doc.Concat(x)
   });
   return this;
  },
  MenuBar$3:function(x)
  {
   this.holes.push({
    $:0,
    $0:"menubar",
    $1:Doc.Concat(x)
   });
   return this;
  },
  MenuBar$4:function(x)
  {
   this.holes.push({
    $:0,
    $0:"menubar",
    $1:x
   });
   return this;
  },
  Title:function(x)
  {
   this.holes.push({
    $:2,
    $0:"title",
    $1:x
   });
   return this;
  },
  Title$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"title",
    $1:x
   });
   return this;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,Main$1);
 Main$1.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Main$1);
 Vars$7=Main$1.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$7);
 Vars$7.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$7);
 Vars$7.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$7);
 Instance$7=Main$1.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$7.New$1(this);
  }
 },TemplateInstance,Instance$7);
 Instance$7.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$7);
 Editablejobitem=Template.Editablejobitem=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.t(completed[0]);
   this.instance=new Instance$8.New(completed[1],doc);
   return this.instance;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,Editablejobitem);
 Editablejobitem.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Editablejobitem);
 Vars$8=Editablejobitem.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$8);
 Vars$8.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$8);
 Vars$8.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$8);
 Instance$8=Editablejobitem.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$8.New$1(this);
  }
 },TemplateInstance,Instance$8);
 Instance$8.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$8);
 EditableJobItem=Editablejobitem.EditableJobItem=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.editablejobitem(completed[0]);
   this.instance=new Instance$9.New(completed[1],doc);
   return this.instance;
  },
  JobParameter:function(x)
  {
   this.holes.push({
    $:2,
    $0:"jobparameter",
    $1:x
   });
   return this;
  },
  JobParameter$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"jobparameter",
    $1:x
   });
   return this;
  },
  JobParameter$2:function(x)
  {
   x=x==void 0?[]:x;
   this.holes.push({
    $:0,
    $0:"jobparameter",
    $1:Doc.Concat(x)
   });
   return this;
  },
  JobParameter$3:function(x)
  {
   this.holes.push({
    $:0,
    $0:"jobparameter",
    $1:Doc.Concat(x)
   });
   return this;
  },
  JobParameter$4:function(x)
  {
   this.holes.push({
    $:0,
    $0:"jobparameter",
    $1:x
   });
   return this;
  },
  Status:function(x)
  {
   this.holes.push({
    $:2,
    $0:"status",
    $1:x
   });
   return this;
  },
  Status$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"status",
    $1:x
   });
   return this;
  },
  QueueName:function(x)
  {
   this.holes.push({
    $:2,
    $0:"queuename",
    $1:x
   });
   return this;
  },
  QueueName$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"queuename",
    $1:x
   });
   return this;
  },
  MethodName:function(x)
  {
   this.holes.push({
    $:2,
    $0:"methodname",
    $1:x
   });
   return this;
  },
  MethodName$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"methodname",
    $1:x
   });
   return this;
  },
  JobGuid:function(x)
  {
   this.holes.push({
    $:2,
    $0:"jobguid",
    $1:x
   });
   return this;
  },
  JobGuid$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"jobguid",
    $1:x
   });
   return this;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,EditableJobItem);
 EditableJobItem.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },EditableJobItem);
 Vars$9=EditableJobItem.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$9);
 Vars$9.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$9);
 Vars$9.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$9);
 Instance$9=EditableJobItem.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$9.New$1(this);
  }
 },TemplateInstance,Instance$9);
 Instance$9.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$9);
 Searchoption=Template.Searchoption=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.t(completed[0]);
   this.instance=new Instance$10.New(completed[1],doc);
   return this.instance;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,Searchoption);
 Searchoption.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Searchoption);
 Vars$10=Searchoption.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$10);
 Vars$10.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$10);
 Vars$10.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$10);
 Instance$10=Searchoption.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$10.New$1(this);
  }
 },TemplateInstance,Instance$10);
 Instance$10.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$10);
 SearchOption=Searchoption.SearchOption=Runtime$1.Class({
  Doc:function()
  {
   return this.Create().get_Doc();
  },
  Create:function()
  {
   var completed,doc;
   completed=Handler.CompleteHoles(this.key,this.holes,[]);
   doc=GlutenFree$OddJob$Manager$Presentation$WS_Templates.searchoption(completed[0]);
   this.instance=new Instance$11.New(completed[1],doc);
   return this.instance;
  },
  JobParameter:function(x)
  {
   this.holes.push({
    $:2,
    $0:"jobparameter",
    $1:x
   });
   return this;
  },
  JobParameter$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"jobparameter",
    $1:x
   });
   return this;
  },
  JobParameter$2:function(x)
  {
   x=x==void 0?[]:x;
   this.holes.push({
    $:0,
    $0:"jobparameter",
    $1:Doc.Concat(x)
   });
   return this;
  },
  JobParameter$3:function(x)
  {
   this.holes.push({
    $:0,
    $0:"jobparameter",
    $1:Doc.Concat(x)
   });
   return this;
  },
  JobParameter$4:function(x)
  {
   this.holes.push({
    $:0,
    $0:"jobparameter",
    $1:x
   });
   return this;
  },
  Status:function(x)
  {
   this.holes.push({
    $:2,
    $0:"status",
    $1:x
   });
   return this;
  },
  Status$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"status",
    $1:x
   });
   return this;
  },
  QueueName:function(x)
  {
   this.holes.push({
    $:2,
    $0:"queuename",
    $1:x
   });
   return this;
  },
  QueueName$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"queuename",
    $1:x
   });
   return this;
  },
  MethodName:function(x)
  {
   this.holes.push({
    $:2,
    $0:"methodname",
    $1:x
   });
   return this;
  },
  MethodName$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"methodname",
    $1:x
   });
   return this;
  },
  JobGuid:function(x)
  {
   this.holes.push({
    $:2,
    $0:"jobguid",
    $1:x
   });
   return this;
  },
  JobGuid$1:function(x)
  {
   this.holes.push({
    $:1,
    $0:"jobguid",
    $1:x
   });
   return this;
  },
  $init:function()
  {
   this.key=Guid.NewGuid();
   this.holes=[];
   this.instance=null;
  }
 },Obj,SearchOption);
 SearchOption.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },SearchOption);
 Vars$11=SearchOption.Vars=Runtime$1.Class({
  $init:function()
  {
   this.instance=null;
  }
 },null,Vars$11);
 Vars$11.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },Vars$11);
 Vars$11.New$1=Runtime$1.Ctor(function(i)
 {
  this.$init();
  this.instance=i;
 },Vars$11);
 Instance$11=SearchOption.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$11.New$1(this);
  }
 },TemplateInstance,Instance$11);
 Instance$11.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$11);
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.t=function(h)
 {
  return h?Templates.GetOrLoadTemplate("jobitem",null,function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<!-- ClientLoad = Inline -->\r\n");
  },h):Templates.PrepareTemplate("jobitem",null,function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<!-- ClientLoad = Inline -->\r\n");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.jobitem=function(h)
 {
  return h?Templates.GetOrLoadTemplate("jobitem",{
   $:1,
   $0:"jobitem"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<span class=\"jobmetadata\" style=\"display: grid\">\r\n        <span>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></span>\r\n        <span>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </span>\r\n        <span>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></span>\r\n        <span>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></span>\r\n</span>");
  },h):Templates.PrepareTemplate("jobitem",{
   $:1,
   $0:"jobitem"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<span class=\"jobmetadata\" style=\"display: grid\">\r\n        <span>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></span>\r\n        <span>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </span>\r\n        <span>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></span>\r\n        <span>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></span>\r\n</span>");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.jobparameter=function(h)
 {
  return h?Templates.GetOrLoadTemplate("jobparameter",{
   $:1,
   $0:"jobparameter"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<div class=\"jobparameter\" ,=\"\" style=\"margin-left: 5%\">\r\n        <div>Ordinal : <input readonly=\"readonly\" value=\"${Ordinal}\"> </div> \r\n        <div>Name: <input value=\"${Name}\" readonly=\"readonly\"> </div> \r\n        <div>Type: <input value=\"${Type}\" readonly=\"readonly\"> </div> \r\n        <div>Value: <input value=\"${Value}\" readonly=\"readonly\"></div>\r\n</div>");
  },h):Templates.PrepareTemplate("jobparameter",{
   $:1,
   $0:"jobparameter"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<div class=\"jobparameter\" ,=\"\" style=\"margin-left: 5%\">\r\n        <div>Ordinal : <input readonly=\"readonly\" value=\"${Ordinal}\"> </div> \r\n        <div>Name: <input value=\"${Name}\" readonly=\"readonly\"> </div> \r\n        <div>Type: <input value=\"${Type}\" readonly=\"readonly\"> </div> \r\n        <div>Value: <input value=\"${Value}\" readonly=\"readonly\"></div>\r\n</div>");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.t$1=function(h)
 {
  return h?Templates.GetOrLoadTemplate("jobsearch",null,function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<html lang=\"en\">\r\n<body>\r\n    <div style=\"width: 400px\">\r\n        <h1>My TODO list</h1>\r\n        <div id=\"search\"></div>\r\n        <div style=\"display: none\"></div>\r\n    </div>\r\n</body>\r\n</html>");
  },h):Templates.PrepareTemplate("jobsearch",null,function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<html lang=\"en\">\r\n<body>\r\n    <div style=\"width: 400px\">\r\n        <h1>My TODO list</h1>\r\n        <div id=\"search\"></div>\r\n        <div style=\"display: none\"></div>\r\n    </div>\r\n</body>\r\n</html>");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.listitem=function(h)
 {
  return h?Templates.GetOrLoadTemplate("jobsearch",{
   $:1,
   $0:"listitem"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n                    <div class=\"checkbox\">\r\n                        <label ws-attr=\"ShowDone\">\r\n                            <!--<input type=\"checkbox\" ws-var=\"Done\" />-->\r\n                            ${Job}\r\n                            <!--<button class=\"btn btn-danger btn-xs pull-right\" type=\"button\" ws-onclick=\"Clear\">X</button>-->\r\n                        </label>\r\n                    </div>\r\n                </li>");
  },h):Templates.PrepareTemplate("jobsearch",{
   $:1,
   $0:"listitem"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n                    <div class=\"checkbox\">\r\n                        <label ws-attr=\"ShowDone\">\r\n                            <!--<input type=\"checkbox\" ws-var=\"Done\" />-->\r\n                            ${Job}\r\n                            <!--<button class=\"btn btn-danger btn-xs pull-right\" type=\"button\" ws-onclick=\"Clear\">X</button>-->\r\n                        </label>\r\n                    </div>\r\n                </li>");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.main=function(h)
 {
  return h?Templates.GetOrLoadTemplate("jobsearch",{
   $:1,
   $0:"main"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("\r\n            <ul class=\"list-unstyled\" ws-hole=\"ListContainer\">\r\n                \r\n            </ul>\r\n            <form onsubmit=\"return false\">\r\n                <div class=\"form-group\">\r\n                    <label>New task</label>\r\n                    <div class=\"input-group\">\r\n                        <input class=\"form-control\" ws-var=\"SearchQueueName\">\r\n                        <input class=\"form-control\" ws-var=\"SearchMethodName\">\r\n                        <span class=\"input-group-btn\">\r\n                            <button class=\"btn btn-primary\" type=\"button\" ws-onclick=\"Search\">Search</button>\r\n                        </span>\r\n                    </div>\r\n                    <p class=\"help-block\">You are going to add: ${NewTaskName}<span></span></p>\r\n                </div>\r\n                <button class=\"btn btn-default\" type=\"button\" ws-onclick=\"Clear\">Clear Criteria</button>\r\n            </form>\r\n        ");
  },h):Templates.PrepareTemplate("jobsearch",{
   $:1,
   $0:"main"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("\r\n            <ul class=\"list-unstyled\" ws-hole=\"ListContainer\">\r\n                \r\n            </ul>\r\n            <form onsubmit=\"return false\">\r\n                <div class=\"form-group\">\r\n                    <label>New task</label>\r\n                    <div class=\"input-group\">\r\n                        <input class=\"form-control\" ws-var=\"SearchQueueName\">\r\n                        <input class=\"form-control\" ws-var=\"SearchMethodName\">\r\n                        <span class=\"input-group-btn\">\r\n                            <button class=\"btn btn-primary\" type=\"button\" ws-onclick=\"Search\">Search</button>\r\n                        </span>\r\n                    </div>\r\n                    <p class=\"help-block\">You are going to add: ${NewTaskName}<span></span></p>\r\n                </div>\r\n                <button class=\"btn btn-default\" type=\"button\" ws-onclick=\"Clear\">Clear Criteria</button>\r\n            </form>\r\n        ");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.t$2=function(h)
 {
  Templates.LoadLocalTemplates("main");
  return h?Templates.NamedTemplate("main",null,h):void 0;
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.editablejobitem=function(h)
 {
  return h?Templates.GetOrLoadTemplate("editablejobitem",{
   $:1,
   $0:"editablejobitem"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"Jobs\">\r\n        <div>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </div>\r\n        <div>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div ws-hole=\"JobParameter\"></div>\r\n    </div>\r\n</li>");
  },h):Templates.PrepareTemplate("editablejobitem",{
   $:1,
   $0:"editablejobitem"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"Jobs\">\r\n        <div>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </div>\r\n        <div>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div ws-hole=\"JobParameter\"></div>\r\n    </div>\r\n</li>");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.searchoption=function(h)
 {
  return h?Templates.GetOrLoadTemplate("searchoption",{
   $:1,
   $0:"searchoption"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"searchOpt\">\r\n        <select></select>\r\n    </div>\r\n    <div class=\"Jobs\">\r\n        <div>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </div>\r\n        <div>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div ws-hole=\"JobParameter\"></div>\r\n    </div>\r\n</li>");
  },h):Templates.PrepareTemplate("searchoption",{
   $:1,
   $0:"searchoption"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"searchOpt\">\r\n        <select></select>\r\n    </div>\r\n    <div class=\"Jobs\">\r\n        <div>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </div>\r\n        <div>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div ws-hole=\"JobParameter\"></div>\r\n    </div>\r\n</li>");
  });
 };
}());

//# sourceMappingURL=GlutenFree.OddJob.Manager.Presentation.WS.map