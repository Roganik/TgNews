using System.Reflection;
using TgNews.BL;
using TgNews.Worker;

IHost host;
var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingCtx, configuration) =>
    {
        if (hostingCtx.HostingEnvironment.IsDevelopment())
        {
            configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
        }

        configuration.AddEnvironmentVariables(prefix: "TGNEWS_");
        configuration.AddJsonFile("subscriptions.json", optional: false);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<SubscriptionsConfigurationSection>(ctx.Configuration.GetSection("Subscriptions"));
    });

if (args.Any(arg => arg.ToLower() == "--server"))
{
    hostBuilder.ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    });
    
    host = hostBuilder.Build();
    await host.RunAsync();
    return;
}

// section for local experiments for cmd-like app
host = hostBuilder.Build();

var subscriptionsCfg = host.Services.GetService<SubscriptionsConfigurationSection>();
var subscriptionsProvider = host.Services.GetService<TgSubscriptionsProvider>();
var subscriptions = subscriptionsProvider.GetAll();

host.WaitForShutdown();


