//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Core;
using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Templating;
using SDoc = WebSharper.UI.Doc;
using DomElement = WebSharper.JavaScript.Dom.Element;
using DomEvent = WebSharper.JavaScript.Dom.Event;
namespace GlutenFree.OddJob.Manager.Presentation.WS.Template
{
    [JavaScript]
    public class Searchoption
    {
        string key = System.Guid.NewGuid().ToString();
        List<TemplateHole> holes = new List<TemplateHole>();
        Instance instance;
        public struct Vars
        {
            public Vars(Instance i) { instance = i; }
            readonly Instance instance;
        }
        public class Instance : WebSharper.UI.Templating.Runtime.Server.TemplateInstance
        {
            public Instance(WebSharper.UI.Templating.Runtime.Server.CompletedHoles v, Doc d) : base(v, d) { }
            public Vars Vars => new Vars(this);
        }
        public Instance Create() {
            var completed = WebSharper.UI.Templating.Runtime.Server.Handler.CompleteHoles(key, holes, new Tuple<string, WebSharper.UI.Templating.Runtime.Server.ValTy>[] {  });
            var doc = WebSharper.UI.Templating.Runtime.Server.Runtime.GetOrLoadTemplate("searchoption", null, FSharpOption<string>.Some("SearchOption.html"), "<!-- ClientLoad = Inline -->\r\n", null, completed.Item1, null, ServerLoad.WhenChanged, new Tuple<string, FSharpOption<string>, string>[] {  }, null, false, false, false);
            instance = new Instance(completed.Item2, doc);
            return instance;
        }
        public Doc Doc() => Create().Doc;
        public class SearchOption
        {
            string key = System.Guid.NewGuid().ToString();
            List<TemplateHole> holes = new List<TemplateHole>();
            Instance instance;
            public SearchOption JobGuid(string x) { holes.Add(TemplateHole.NewText("jobguid", x)); return this; }
            public SearchOption JobGuid(View<string> x) { holes.Add(TemplateHole.NewTextView("jobguid", x)); return this; }
            public SearchOption MethodName(string x) { holes.Add(TemplateHole.NewText("methodname", x)); return this; }
            public SearchOption MethodName(View<string> x) { holes.Add(TemplateHole.NewTextView("methodname", x)); return this; }
            public SearchOption QueueName(string x) { holes.Add(TemplateHole.NewText("queuename", x)); return this; }
            public SearchOption QueueName(View<string> x) { holes.Add(TemplateHole.NewTextView("queuename", x)); return this; }
            public SearchOption Status(string x) { holes.Add(TemplateHole.NewText("status", x)); return this; }
            public SearchOption Status(View<string> x) { holes.Add(TemplateHole.NewTextView("status", x)); return this; }
            public SearchOption JobParameter(Doc x) { holes.Add(TemplateHole.NewElt("jobparameter", x)); return this; }
            public SearchOption JobParameter(IEnumerable<Doc> x) { holes.Add(TemplateHole.NewElt("jobparameter", SDoc.Concat(x))); return this; }
            public SearchOption JobParameter(params Doc[] x) { holes.Add(TemplateHole.NewElt("jobparameter", SDoc.Concat(x))); return this; }
            public SearchOption JobParameter(string x) { holes.Add(TemplateHole.NewText("jobparameter", x)); return this; }
            public SearchOption JobParameter(View<string> x) { holes.Add(TemplateHole.NewTextView("jobparameter", x)); return this; }
            public struct Vars
            {
                public Vars(Instance i) { instance = i; }
                readonly Instance instance;
            }
            public class Instance : WebSharper.UI.Templating.Runtime.Server.TemplateInstance
            {
                public Instance(WebSharper.UI.Templating.Runtime.Server.CompletedHoles v, Doc d) : base(v, d) { }
                public Vars Vars => new Vars(this);
            }
            public Instance Create() {
                var completed = WebSharper.UI.Templating.Runtime.Server.Handler.CompleteHoles(key, holes, new Tuple<string, WebSharper.UI.Templating.Runtime.Server.ValTy>[] {  });
                var doc = WebSharper.UI.Templating.Runtime.Server.Runtime.GetOrLoadTemplate("searchoption", FSharpOption<string>.Some("searchoption"), FSharpOption<string>.Some("SearchOption.html"), "<li>\r\n    <div class=\"searchOpt\">\r\n        <select></select>\r\n    </div>\r\n    <div class=\"Jobs\">\r\n        <div>JobGuid: <input value=\"${JobGuid}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>MethodName: <input value=\"${MethodName}\" readonly=\"readonly\" size=\"40\"> </div>\r\n        <div>QueueName: <input value=\"${QueueName}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div>Status: <input value=\"${Status}\" readonly=\"readonly\" size=\"40\"></div>\r\n        <div ws-hole=\"JobParameter\"></div>\r\n    </div>\r\n</li>", null, completed.Item1, null, ServerLoad.WhenChanged, new Tuple<string, FSharpOption<string>, string>[] {  }, null, true, false, false);
                instance = new Instance(completed.Item2, doc);
                return instance;
            }
            public Doc Doc() => Create().Doc;
            [Inline] public Elt Elt() => (Elt)Doc();
        }
    }
}