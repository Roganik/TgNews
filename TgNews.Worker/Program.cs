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

// section for local cmd-like development
host = hostBuilder.Build();
var cfg = host.Services.GetService<IConfiguration>();
var tgCfg = new TgNewsConfiguration(cfg);

var db = new TgNews.BL.Client.DbStorage(tgCfg);
var tg = new TgNews.BL.Client.Telegram();
var tgBot = new TgNews.BL.Client.TelegramBot();

await tg.Init(tgCfg);
await tgBot.Init(tgCfg);

var forwarder = new Forwarder(tg, tgBot, db);
await forwarder.Execute();

Console.WriteLine("Press any key to quit");


