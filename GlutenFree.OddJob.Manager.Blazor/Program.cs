using GlutenFree.Linq2Db.FluentMigrator.Helpers;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Manager.Blazor;
using GlutenFree.OddJob.Manager.Blazor.Components;
using GlutenFree.OddJob.Manager.Blazor.Helpers;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// Register controllers for API endpoints, uwu!
builder.Services.AddControllers();

// OddJob DI setup (Ami-chan magic, uwu!)
// Use SQLite by default; swap to SqlServerDataConnectionFactory if needed
builder.Services.AddSingleton<SqlDbJobQueueDefaultTableConfiguration>(new SqlDbJobQueueDefaultTableConfiguration() { });
builder.Services.AddScoped<OddJobRemotingHandler>();
builder.Services.AddScoped<IJobQueueAdder, SQLiteJobQueueAdder>();
builder.Services.AddScoped<ISerializedJobQueueAdder, SQLiteJobQueueAdder>();
builder.Services.AddScoped<IJobSearchProvider, SqlDbJobSearchProvider>();
builder.Services.AddScoped<SQLiteJobQueueDataConnectionFactory>(sp =>
    new SQLiteJobQueueDataConnectionFactory(
        sqlitehelperclass.EnsureConnecttionStringExists(
            SQLiteUnitTestTableHelper.connString
    )));
builder.Services.AddScoped<IJobQueueDataConnectionFactory>(sp =>
    sp.GetRequiredService<SQLiteJobQueueDataConnectionFactory>());
builder.Services.AddScoped<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
builder.Services.AddScoped<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();
builder.Services.AddScoped<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>((sp) =>
    new DefaultJobAdderQueueTableResolver(sp.GetRequiredService<SqlDbJobQueueDefaultTableConfiguration>()));
builder.Services.AddRazorPages();
//builder.Services.AddScoped(sp => 
//    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//builder.Services.AddHttpClient("ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddHttpClient("ServerAPI", client => client.BaseAddress = new Uri(builder.Configuration["ServerApiBaseAddress"] ?? "http://localhost:5269"));
var app = builder.Build();

// --- Ami-chan migration magic, uwu! ---
using (var s = app.Services.CreateScope())
{
    SQLiteUnitTestTableHelper.EnsureTablesExist();
    // Get the connection factory from DI
    var connFactory = s.ServiceProvider.GetRequiredService<SQLiteJobQueueDataConnectionFactory>();
    var tableConfig = s.ServiceProvider.GetRequiredService<SqlDbJobQueueDefaultTableConfiguration>();
    // Use the migration helper to create tables if needed
    // Warm up OddJob services (add a sample job)
    var sjqa = s.ServiceProvider.GetService<IJobQueueAdder>();
    sjqa.AddJob((SampleJob j) => j.DoThing(new SampleData() { Id = 1 }));
    sjqa.AddJob((SampleJob j) => j.DoThing(new SampleData() { Id = 2 }));
    sjqa.AddJob((SampleJob2 j) => j.DoThing(new MoreSampleData() { Name = "Ami-chan" }));
    sjqa.AddJob((SampleJob2 j) => j.DoOtherThing(new SampleData() { Id = 3 }, new MoreSampleData() { Name = "Nyaa~" }));
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
// Map API controllers so Blazor can talk to them, nyaa~!

app.MapRazorPages();
app.MapFallbackToPage("/_Host");
app.Run();