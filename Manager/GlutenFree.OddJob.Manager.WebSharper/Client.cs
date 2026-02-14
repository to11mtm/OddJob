using System.Threading.Tasks;
using WebSharper;
using WebSharper.UI;
using Html = WebSharper.UI.Client.Html;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    [JavaScript]
    public static class Client
    {
        static public IControlBody Main()
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
    }
}