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
builder.Services.AddScoped<OddJobRemotingHandler>();
builder.Services.AddScoped<IJobSearchProvider, SqlDbJobSearchProvider>();
builder.Services.AddScoped<IJobQueueDataConnectionFactory>(sp =>
    new SQLiteJobQueueDataConnectionFactory(
        builder.Configuration.GetConnectionString("OddJobDb") ?? "Data Source=oddjob.db;Version=3;"
    ));
builder.Services.AddScoped<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
builder.Services.AddScoped<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();

var app = builder.Build();

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