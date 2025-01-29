using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Example;
using Hangfire.Example.Filters;
using Hangfire.Example.Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var sqlServerOptions =
    new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(5),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    };

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), sqlServerOptions));

// Needed so that in AppHost the Recurring Job can be added. I don't know why, it should not be necessary!
GlobalConfiguration.Configuration.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"),
    sqlServerOptions);

// Add Hangfire Server and it's now Service Activator!
builder.Services.AddHangfireServer(options =>
{
    options.Activator = new ContainerJobActivator(HostContext.AppHost.Container);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

var hangfireDashboardConfig = builder.Configuration.GetSection("Hangfire");

app.UseServiceStack(new AppHost(app.Configuration));


app.UseHangfireDashboard("/dashboard", options: new DashboardOptions
{
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangfireHttpBasicAuthenticationFilter(hangfireDashboardConfig["Username"] ?? string.Empty,
            hangfireDashboardConfig["Password"] ?? string.Empty)
    }
});

app.Run();