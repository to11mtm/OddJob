using System.Threading.Tasks;
using GlutenFree.OddJob.Manager.Presentation.WS.Template;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    public class Server
    {
        [EndPoint("/")]
        public class Home
        {
            public override bool Equals(object obj) => obj is Home;
            public override int GetHashCode() => 0;
        }

        [EndPoint("GET /about")]
        public class About
        {
            public override bool Equals(object obj) => obj is About;
            public override int GetHashCode() => 1;
        }

        public static Doc MenuBar(Context<object> ctx, object endpoint)
        {
            Doc link(string txt, object act) =>
                Html.li(
                    endpoint.Equals(act) ? Html.attr.@class("active") : null,
                    Html.a(Html.attr.href(ctx.Link(act)), txt)
                );
            return Html.doc(
                Html.li(link("Home", new Home())),
                Html.li(link("About", new About()))
            );
        }

        public static Task<Content> Page(Context<object> ctx, object endpoint, string title, Doc body) =>
            Content.Page(
                new Main()
                    .Title(title)
                    .MenuBar(MenuBar(ctx, endpoint))
                    .Body(body)
                    .Doc()
            );

        [Website]
        public static Sitelet<object> Main =>
            new SiteletBuilder()
                .With<Home>((ctx, action) =>
                    Page(ctx, action, "Home",
                        Html.doc(
                            Html.h1("Say Hi to the server!"),
                            Html.div(Html.client(() => Client.Main()))
                        )
                    )
                )
                .With<About>((ctx, action) =>
                    Page(ctx, action, "About",
                        Html.doc(
                            Html.h1("About"),
                            Html.p("This is a template WebSharper client-server application.")
                        )
                    )
                )
                .Install();
    }
}