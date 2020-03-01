using System.Threading.Tasks;
using GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore.Template;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI;
using static WebSharper.UI.Html;

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
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
                li(
                    endpoint.Equals(act) ? attr.@class("active") : null,
                    a(attr.href(ctx.Link(act)), txt)
                );
            return doc(
                li(link("Home", new Home())),
                li(link("About", new About()))
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
        public Sitelet<object> Main =>
            new SiteletBuilder()
                .With<Home>((ctx, action) =>
                    Page(ctx, action, "Home",
                        doc(
                            h1("Say Hi to the server!"),
                            div(client(() => JobSearchClient.Main()))
                        )
                    )
                )
                .With<About>((ctx, action) =>
                    Page(ctx, action, "About",
                        doc(
                            h1("About"),
                            p("This is a Sample Management application for OddJob."),
                            p("You may customize it and utilize your own DB connection instead"),
                            p("And also play with the Styling as desired.")
                        )
                    )
                )
                .Install();
    }
}