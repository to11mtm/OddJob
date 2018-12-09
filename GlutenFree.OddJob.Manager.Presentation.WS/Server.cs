using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using GlutenFree.OddJob.Manager.Presentation.WS.Template;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using GlutenFree.OddJob.Storage.SQL.SQLite;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI;
using static WebSharper.UI.Html;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    public interface IJob1
    {
        void Job1Method1();
        void Job1Method2();
    }

    public interface IJob2
    {
        void Job2Method1();
        void Job2Method2(string param1);
    }

    public static class TempDevInfo
    {
        internal static readonly string
            ConnString = "FullURI=file::memory:?cache=shared"; //"FullUri=file::memory:?cache=shared";

        /// <summary>
        /// This is here because SQLite will only hold In-memory DBs as long as ONE connection is open. so we just open one here and keep it around for appdomain life.
        /// </summary>
        public static readonly SQLiteConnection heldConnection;

        public static bool TablesCreated = false;

        static TempDevInfo()
        {
            heldConnection = new SQLiteConnection(ConnString);
            EnsureTablesExist(TableConfigurations.Values.Append(new SqlDbJobQueueDefaultTableConfiguration()));
            var sampleDataAdder1 = new SQLiteJobQueueAdder(new SQLiteJobQueueDataConnectionFactory(ConnString),
                new QueueNameBasedJobAdderQueueTableResolver(TableConfigurations,
                    new SqlDbJobQueueDefaultTableConfiguration()));
            sampleDataAdder1.AddJob((IJob1 j) => j.Job1Method1(), new RetryParameters(), null, "console");
            sampleDataAdder1.AddJob((IJob1 j) => j.Job1Method2(), new RetryParameters(), null, "console");
            sampleDataAdder1.AddJob((IJob1 j) => j.Job1Method1(), new RetryParameters(), null, "counter");
            sampleDataAdder1.AddJob((IJob2 j) => j.Job2Method1(), new RetryParameters(), null, "counter");
            sampleDataAdder1.AddJob((IJob2 j) => j.Job2Method2("derp"), new RetryParameters(), queueName: "console");
        }

        //We only want this to execute once for the sample.
        //You would want to get rid of the DROPs in a real world scenario, and instead check that you aren't breaking anything.
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void EnsureTablesExist(IEnumerable<ISqlDbJobQueueTableConfiguration> configs)
        {
            if (TablesCreated)
            {
                return;
                ;
            }

            if (heldConnection.State != ConnectionState.Open)
            {
                heldConnection.Open();
            }

            foreach (var tableConfiguration in configs)
            {
                using (var db = new SQLiteConnection(ConnString))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                            tableConfiguration.QueueTableName);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                            tableConfiguration.ParamTableName);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                            tableConfiguration.JobMethodGenericParamTableName);
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = SQLiteDbJobTableHelper.JobQueueParamTableCreateScript(
                            tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = SQLiteDbJobTableHelper.JobTableCreateScript(
                            tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText =
                            SQLiteDbJobTableHelper.JobQueueJobMethodGenericParamTableCreateScript(
                                tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText =
                            SQLiteDbJobTableHelper.SuggestedIndexes(tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                }
            }


            TablesCreated = true;




        }

        public static Dictionary<string, ISqlDbJobQueueTableConfiguration> TableConfigurations
        {
            get
            {
                return new Dictionary<string, ISqlDbJobQueueTableConfiguration>()
                {
                    {
                        "console",
                        new MyTableConfigs()
                        {
                            QueueTableName = "consoleQueue", ParamTableName = "consoleParam",
                            JobMethodGenericParamTableName = "consoleGeneric", JobClaimLockTimeoutInSeconds = 30
                        }
                    },
                    {
                        "counter",
                        new MyTableConfigs()
                        {
                            QueueTableName = "counterQueue", ParamTableName = "counterParam",
                            JobMethodGenericParamTableName = "counterGeneric", JobClaimLockTimeoutInSeconds = 30
                        }
                    }
                };
            }
        }


    }


    public class MyTableConfigs : ISqlDbJobQueueTableConfiguration
    {
        public string QueueTableName { get; set; }
        public string ParamTableName { get; set; }
        public int JobClaimLockTimeoutInSeconds { get; set; }
        public string JobMethodGenericParamTableName { get; set; }
    }
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

        [EndPoint("GET /search")]
        public class Search
        {
            public override bool Equals(object obj) => obj is Search;
            public override int GetHashCode() => 2;
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
                li(link("About", new About())),
                li(link("Search",new Search()))
            );
        }

        public static Task<Content> Page(Context<object> ctx, object endpoint, string title, Doc body) =>
            Content.Page(
                new Template.Main()
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
                        doc(
                            h1("Say Hi to the server!"),
                            div(client(() => Client.Main()))
                        )
                    )
                )
                .With<About>((ctx, action) =>
                    Page(ctx, action, "About",
                        doc(
                            h1("About"),
                            p("This is a template WebSharper client-server application.")
                        )
                    )
                )
                .With<Search>((ctx,action)=>
                    Page(ctx,action,"Search",
                        doc(
                            h1("Search"),
                            div(client(()=> JobSearchClient.Main()))))
                    )
                .Install();
    }
}