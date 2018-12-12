(function()
{
 "use strict";
 var Global,GlutenFree,OddJob,Manager,Presentation,WS,JobSearchClient,Client,WebSharper,Obj,JobSearchCriteria,JobRetryParameters,JobParameterDto,JobMetadataResult,Template,Jobitem,Vars,UI,Templating,Runtime,Server,TemplateInstance,Instance,JobItem,Vars$1,Instance$1,Jobparameter,Vars$2,Instance$2,JobParameter,Vars$3,Instance$3,Jobsearch,Vars$4,Instance$4,ListItem,Vars$5,Instance$5,Main,Vars$6,Instance$6,Main$1,Vars$7,Instance$7,Searchoption,Vars$8,Instance$8,SearchOption,Vars$9,Instance$9,GlutenFree$OddJob$Manager$Presentation$WS_Templates,Var$1,Submitter,Unchecked,Doc,Remoting,AjaxRemotingProvider,Arrays,Seq,Linq,View,Concurrency,List,AttrModule,AttrProxy,IntelliFactory,Runtime$1,Task,Handler,System,Guid,Client$1,Templates,DomUtility;
 Global=self;
 GlutenFree=Global.GlutenFree=Global.GlutenFree||{};
 OddJob=GlutenFree.OddJob=GlutenFree.OddJob||{};
 Manager=OddJob.Manager=OddJob.Manager||{};
 Presentation=Manager.Presentation=Manager.Presentation||{};
 WS=Presentation.WS=Presentation.WS||{};
 JobSearchClient=WS.JobSearchClient=WS.JobSearchClient||{};
 Client=WS.Client=WS.Client||{};
 WebSharper=Global.WebSharper;
 Obj=WebSharper&&WebSharper.Obj;
 JobSearchCriteria=WS.JobSearchCriteria=WS.JobSearchCriteria||{};
 JobRetryParameters=WS.JobRetryParameters=WS.JobRetryParameters||{};
 JobParameterDto=WS.JobParameterDto=WS.JobParameterDto||{};
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
 Searchoption=Template.Searchoption=Template.Searchoption||{};
 Vars$8=Searchoption.Vars=Searchoption.Vars||{};
 Instance$8=Searchoption.Instance=Searchoption.Instance||{};
 SearchOption=Searchoption.SearchOption=Searchoption.SearchOption||{};
 Vars$9=SearchOption.Vars=SearchOption.Vars||{};
 Instance$9=SearchOption.Instance=SearchOption.Instance||{};
 GlutenFree$OddJob$Manager$Presentation$WS_Templates=Global.GlutenFree$OddJob$Manager$Presentation$WS_Templates=Global.GlutenFree$OddJob$Manager$Presentation$WS_Templates||{};
 Var$1=UI&&UI.Var$1;
 Submitter=UI&&UI.Submitter;
 Unchecked=WebSharper&&WebSharper.Unchecked;
 Doc=UI&&UI.Doc;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 Arrays=WebSharper&&WebSharper.Arrays;
 Seq=WebSharper&&WebSharper.Seq;
 Linq=WebSharper&&WebSharper.Linq;
 View=UI&&UI.View;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 List=WebSharper&&WebSharper.List;
 AttrModule=UI&&UI.AttrModule;
 AttrProxy=UI&&UI.AttrProxy;
 IntelliFactory=Global.IntelliFactory;
 Runtime$1=IntelliFactory&&IntelliFactory.Runtime;
 Task=WebSharper&&WebSharper.Task;
 Handler=Server&&Server.Handler;
 System=Global.System;
 Guid=System&&System.Guid;
 Client$1=UI&&UI.Client;
 Templates=Client$1&&Client$1.Templates;
 DomUtility=UI&&UI.DomUtility;
 JobSearchClient.Main=function()
 {
  var criteria,a,statusLens,a$1,useStatus,a$2,methodLens,a$3,useMethod,statusOptions,methodCriteria,submit,results,a$4,queueNameLens,callback,content;
  criteria=Var$1.Create$1(new JobSearchCriteria.New());
  function del(a$5,b)
  {
   a$5.set_Status(b);
   return a$5;
  }
  statusLens=(a=function(a$5)
  {
   return function(b)
   {
    return del(a$5,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.get_Status();
  },function($1,$2)
  {
   return(a($1))($2);
  }));
  function del$1(a$5,b)
  {
   a$5.UseStatus=b;
   return a$5;
  }
  useStatus=(a$1=function(a$5)
  {
   return function(b)
   {
    return del$1(a$5,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.UseStatus;
  },function($1,$2)
  {
   return(a$1($1))($2);
  }));
  function del$2(a$5,b)
  {
   a$5.set_MethodName(b);
   return a$5;
  }
  methodLens=(a$2=function(a$5)
  {
   return function(b)
   {
    return del$2(a$5,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.get_MethodName();
  },function($1,$2)
  {
   return(a$2($1))($2);
  }));
  function del$3(a$5,b)
  {
   a$5.UseMethod=b;
   return a$5;
  }
  useMethod=(a$3=function(a$5)
  {
   return function(b)
   {
    return del$3(a$5,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.UseMethod;
  },function($1,$2)
  {
   return(a$3($1))($2);
  }));
  statusOptions=Var$1.Create$1([null,"Processed","New","Failed","Retry","InProgress","Inserting"]);
  methodCriteria=Var$1.Create$1([null]);
  submit=Submitter.CreateOption(criteria.get_View());
  function f(input)
  {
   var $task,$run,$state,methodOptionFuture,$await,awaitedMethodOptions,future,$await$1,awaitedFuture,result;
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
       result=Arrays.ofSeq(Seq.map(function(q)
       {
        return(new JobItem.New()).MethodName$1(q.get_MethodName()).QueueName$1(q.get_Queue()).Status$1(q.get_Status()).JobGuid$1(q.get_JobId()).JobParameter$4(Doc.ElementMixed("ul",Arrays.ofSeq(Linq.Select(q.get_JobArgs(),function(r,i)
        {
         return(new JobParameter.New()).Type$1(r.get_Type()).Name$1(r.get_Name()).Value$1(r.get_Value()).Ordinal$1(Global.String(i)).Doc();
        })))).Doc();
       },awaitedFuture));
       $task.result=Doc.ElementMixed("div",[Doc.ElementMixed("h3",["Results:"]),Doc.ElementMixed("br",[]),Doc.ConcatMixed([Doc.ElementMixed("ul",result)])]);
       $task.status=5;
       $task.RunContinuations();
       return;
     }
   };
   $run();
   return $task;
  }
  results=View.MapAsync(function(a$5)
  {
   return Concurrency.AwaitTask1(f(a$5));
  },submit.view);
  function del$4(a$5,b)
  {
   a$5.set_QueueName(b);
   return a$5;
  }
  queueNameLens=(a$4=function(a$5)
  {
   return function(b)
   {
    return del$4(a$5,b);
   };
  },Var$1.Lens(criteria,function(q)
  {
   return q.get_QueueName();
  },function($1,$2)
  {
   return(a$4($1))($2);
  }));
  function del$5(r,e)
  {
   return submit.Trigger();
  }
  content=Doc.ElementMixed("div",[Doc.ElementMixed("div",["Queue Name: ",Doc.Select([],function(q)
  {
   return q===null?"Please Select a Queue...":q;
  },List.ofSeq([null,"console","counter"]),queueNameLens),AttrModule.Handler("change",function(a$5)
  {
   return function(b)
   {
    return del$5(a$5,b);
   };
  }),AttrProxy.Create("name","queueNameSelect")]),JobSearchClient.TextSearch("Method Name",methodLens,useMethod),JobSearchClient.OptionSearch("Status",statusLens,statusOptions.get_View(),useStatus,function()
  {
   return submit.Trigger();
  }),JobSearchClient.OptionSearch("Method",methodLens,methodCriteria.get_View(),useMethod,function()
  {
   return submit.Trigger();
  }),(callback=Runtime$1.BindDelegate(Submitter.prototype.Trigger,submit),Doc.Button("Search",[],function()
  {
   callback();
  })),Doc.ElementMixed("div",[results])]);
  return content;
 };
 JobSearchClient.OptionSearch=function(name,criteriaLens,optionView,useCriteriaLens,changeAction)
 {
  var del;
  return Doc.ElementMixed("div",[Doc.CheckBox([],useCriteriaLens),name+": ",Doc.SelectDyn([],function(q)
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
 JobSearchClient.TextSearch=function(name,criteriaLens,useCriteriaLens)
 {
  return Doc.ElementMixed("div",[Doc.CheckBox([],useCriteriaLens),name,Doc.Input([],criteriaLens)]);
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
 JobRetryParameters=WS.JobRetryParameters=Runtime$1.Class({
  set_LastAttempt:function(value)
  {
   this.$LastAttempt=value;
  },
  get_LastAttempt:function()
  {
   return this.$LastAttempt;
  },
  set_RetryCount:function(value)
  {
   this.$RetryCount=value;
  },
  get_RetryCount:function()
  {
   return this.$RetryCount;
  },
  set_MinRetryWait:function(value)
  {
   this.$MinRetryWait=value;
  },
  get_MinRetryWait:function()
  {
   return this.$MinRetryWait;
  },
  set_MaxRetries:function(value)
  {
   this.$MaxRetries=value;
  },
  get_MaxRetries:function()
  {
   return this.$MaxRetries;
  },
  $init:function()
  {
   this.set_MaxRetries(0);
   this.set_MinRetryWait(0);
   this.set_RetryCount(0);
   this.set_LastAttempt(null);
  }
 },Obj,JobRetryParameters);
 JobRetryParameters.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobRetryParameters);
 JobParameterDto=WS.JobParameterDto=Runtime$1.Class({
  set_Value:function(value)
  {
   this.$Value=value;
  },
  get_Value:function()
  {
   return this.$Value;
  },
  set_Name:function(value)
  {
   this.$Name=value;
  },
  get_Name:function()
  {
   return this.$Name;
  },
  set_Type:function(value)
  {
   this.$Type=value;
  },
  get_Type:function()
  {
   return this.$Type;
  },
  $init:function()
  {
   this.set_Type(null);
   this.set_Name(null);
   this.set_Value(null);
  }
 },Obj,JobParameterDto);
 JobParameterDto.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },JobParameterDto);
 JobMetadataResult=WS.JobMetadataResult=Runtime$1.Class({
  set_Queue:function(value)
  {
   this.$Queue=value;
  },
  get_Queue:function()
  {
   return this.$Queue;
  },
  set_RetryParameters:function(value)
  {
   this.$RetryParameters=value;
  },
  get_RetryParameters:function()
  {
   return this.$RetryParameters;
  },
  set_ExecutionTime:function(value)
  {
   this.$ExecutionTime=value;
  },
  get_ExecutionTime:function()
  {
   return this.$ExecutionTime;
  },
  set_MethodGenericTypes:function(value)
  {
   this.$MethodGenericTypes=value;
  },
  get_MethodGenericTypes:function()
  {
   return this.$MethodGenericTypes;
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
  set_TypeExecutedOn:function(value)
  {
   this.$TypeExecutedOn=value;
  },
  get_TypeExecutedOn:function()
  {
   return this.$TypeExecutedOn;
  },
  set_JobArgs:function(value)
  {
   this.$JobArgs=value;
  },
  get_JobArgs:function()
  {
   return this.$JobArgs;
  },
  set_JobId:function(value)
  {
   this.$JobId=value;
  },
  get_JobId:function()
  {
   return this.$JobId;
  },
  $init:function()
  {
   this.set_JobId(null);
   this.set_JobArgs(null);
   this.set_TypeExecutedOn(null);
   this.set_MethodName(null);
   this.set_Status(null);
   this.set_MethodGenericTypes(null);
   this.set_ExecutionTime(null);
   this.set_RetryParameters(null);
   this.set_Queue(null);
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
   this.instance=new Instance$8.New(completed[1],doc);
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
 Vars$8=Searchoption.Vars=Runtime$1.Class({
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
 Instance$8=Searchoption.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$8.New$1(this);
  }
 },TemplateInstance,Instance$8);
 Instance$8.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$8);
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
 },Obj,SearchOption);
 SearchOption.New=Runtime$1.Ctor(function()
 {
  this.$init();
 },SearchOption);
 Vars$9=SearchOption.Vars=Runtime$1.Class({
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
 Instance$9=SearchOption.Instance=Runtime$1.Class({
  get_Vars:function()
  {
   return new Vars$9.New$1(this);
  }
 },TemplateInstance,Instance$9);
 Instance$9.New=Runtime$1.Ctor(function(v,d)
 {
  TemplateInstance.New.call(this,v,d);
 },Instance$9);
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
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"Jobs\">\r\n        <div>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </div>\r\n        <div>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div ws-hole=\"JobParameter\"></div>\r\n    </div>\r\n</li>");
  },h):Templates.PrepareTemplate("jobitem",{
   $:1,
   $0:"jobitem"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"Jobs\">\r\n        <div>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </div>\r\n        <div>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div ws-hole=\"JobParameter\"></div>\r\n    </div>\r\n</li>");
  });
 };
 GlutenFree$OddJob$Manager$Presentation$WS_Templates.jobparameter=function(h)
 {
  return h?Templates.GetOrLoadTemplate("jobparameter",{
   $:1,
   $0:"jobparameter"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"JobParameter\">\r\n        <div>Ordinal : <input readonly=\"readonly\" value=\"${Ordinal}\"> </div> <div>Name: <input value=\"${Name}\" readonly=\"readonly\"> </div> <div>Type: <input value=\"${Type}\" readonly=\"readonly\"> </div> <div>Value: <input value=\"${Value}\" readonly=\"readonly\"></div>\r\n    </div>\r\n</li>");
  },h):Templates.PrepareTemplate("jobparameter",{
   $:1,
   $0:"jobparameter"
  },function()
  {
   return DomUtility.ParseHTMLIntoFakeRoot("<li>\r\n    <div class=\"JobParameter\">\r\n        <div>Ordinal : <input readonly=\"readonly\" value=\"${Ordinal}\"> </div> <div>Name: <input value=\"${Name}\" readonly=\"readonly\"> </div> <div>Type: <input value=\"${Type}\" readonly=\"readonly\"> </div> <div>Value: <input value=\"${Value}\" readonly=\"readonly\"></div>\r\n    </div>\r\n</li>");
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