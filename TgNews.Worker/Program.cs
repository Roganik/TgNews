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
            configuration.AddJsonFile("subscriptions.Development.json");
        }

        configuration.AddEnvironmentVariables(prefix: "TGNEWS_");
        configuration.AddJsonFile("subscriptions.json", optional: false);

    })
    .ConfigureLogging((ctx, logging) =>
    {
        var sentryDsn = ctx.Configuration.GetValue<string>("Sentry:Dsn");
        if (string.IsNullOrEmpty(sentryDsn))
        {
            return;
        }
        
        logging.AddSentry(o =>
        {
            o.Dsn = sentryDsn;
            o.MinimumBreadcrumbLevel = LogLevel.Debug;
            o.MinimumEventLevel = LogLevel.Warning;
            o.MaxBreadcrumbs = 50;
        });
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddSingleton<SubscriptionsConfiguration>(_ => ctx.Configuration.Get<SubscriptionsConfiguration>());
        services.AddSingleton<TgSubscriptionsProvider>();
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

var iCfg = host.Services.GetService<IConfiguration>();
var typed = iCfg.Get<SubscriptionsConfiguration>();

var subscriptionsCfg = host.Services.GetService<SubscriptionsConfiguration>();
var subscriptionsProvider = host.Services.GetService<TgSubscriptionsProvider>();
var subscriptions = subscriptionsProvider.GetAll();

host.WaitForShutdown();


