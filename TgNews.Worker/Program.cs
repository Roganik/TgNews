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
var iCfg = host.Services.GetService<IConfiguration>();
var cfg = new TgNewsConfiguration(iCfg);

var db = new TgNews.BL.Client.DbStorage(cfg);
var tg = new TgNews.BL.Client.Telegram(cfg);
var tgBot = new TgNews.BL.Client.TelegramBot(cfg);

await tg.Init();
await tgBot.Init();

var forwarder = new Forwarder(tg, tgBot, db);
await forwarder.Execute();

Console.WriteLine("Press any key to quit");


