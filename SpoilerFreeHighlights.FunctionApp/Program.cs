using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        //Environment.SetEnvironmentVariable("APP_BASE_DIR", AppContext.BaseDirectory, EnvironmentVariableTarget.Process);

        Log.Logger = new LoggerConfiguration()
            //.ReadFrom.Configuration(context.Configuration)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateLogger();
        
        services.AddLogging(x => x.AddSerilog(dispose: true));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddHttpClient("Default", client =>
        {
            client.BaseAddress = new Uri(context.Configuration.GetValue("BaseAddress", "http://localhost:7079"));
        });

        services.SetupDatabase(context.Configuration, false);
        
        services.AddServices();
    });

IHost host = builder
    .Build();

await host.Services.ApplyMigrationsAndSeedDatabase(false);

host.Run();
