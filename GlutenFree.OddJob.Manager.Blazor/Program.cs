using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Manager.Blazor;
using GlutenFree.OddJob.Manager.Blazor.Components;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// OddJob DI setup (Ami-chan magic, uwu!)
// Use SQLite by default; swap to SqlServerDataConnectionFactory if needed
builder.Services.AddSingleton<SqlDbJobQueueDefaultTableConfiguration>(new SqlDbJobQueueDefaultTableConfiguration() { });
builder.Services.AddScoped<OddJobRemotingHandler>();
builder.Services.AddScoped<IJobQueueAdder, SQLiteJobQueueAdder>();
builder.Services.AddScoped<ISerializedJobQueueAdder, SQLiteJobQueueAdder>();
builder.Services.AddScoped<IJobSearchProvider, SqlDbJobSearchProvider>();
builder.Services.AddScoped<SQLiteJobQueueDataConnectionFactory>(sp =>
    new SQLiteJobQueueDataConnectionFactory(
        "Data Source=oddjob.db;Version=3;"
    ));
builder.Services.AddScoped<IJobQueueDataConnectionFactory>(sp =>
    sp.GetRequiredService<SQLiteJobQueueDataConnectionFactory>());
builder.Services.AddScoped<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
builder.Services.AddScoped<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();
builder.Services.AddScoped<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>((sp) =>
    new DefaultJobAdderQueueTableResolver(sp.GetRequiredService<SqlDbJobQueueDefaultTableConfiguration>()));
builder.Services.AddHttpClient();
var app = builder.Build();
using (var s = app.Services.CreateScope())
{
    var sjqa = s.ServiceProvider.GetService<IJobQueueAdder>(); // Warm up OddJob services
    sjqa.AddJob((SampleJob j) => j.DoThing(new SampleData() { Id = 1 }));
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();