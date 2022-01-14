using System.Reflection;
using Newtonsoft.Json;
using TgNews.BL;
using TgNews.Worker;
using TL;

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

// section for local experiments for cmd-like app
host = hostBuilder.Build();
var iCfg = host.Services.GetService<IConfiguration>();
var cfg = new TgNewsConfiguration(iCfg);
var logger = host.Services.GetService<ILogger<Program>>();

var db = new TgNews.BL.Client.DbStorage(cfg);
var tg = new TgNews.BL.Client.Telegram(cfg);
var tgBot = new TgNews.BL.Client.TelegramBot(cfg);

var eventHandler = (IObject arg) =>
{
    if (arg is not UpdatesBase updates)
    {
        logger.LogWarning("Unknown event type, skipping");
        logger.LogWarning(arg.GetType().ToString());
        return;
    }

    var updatedChats = string.Join(";", updates.Chats.Select(c => c.Value.Title));
    if (!string.IsNullOrEmpty(updatedChats))
    {
        logger.LogCritical("UpdatedChats:  " + updatedChats);
    }

    foreach (var update in updates.UpdateList)
    {
        var logEvent = () =>
        {
            var type = update.GetType();
            var json = JsonConvert.SerializeObject(update, type,
                new JsonSerializerSettings() {Formatting = Formatting.Indented});
            ;
            logger.LogWarning(type.Name);
            logger.LogInformation(json);
        };
        switch (update)
        {
            case UpdateUserStatus: break;
            case UpdateMessagePoll: break;
            default:
                logEvent();
                break;
        }
    }
};

var typedUpdates = new WTelegram.TypedUpdates();
typedUpdates.OnUpdateEditChannelMessage = (m) =>
{
    m.
};

await tg.Init(typedUpdates);
await tgBot.Init();

Console.WriteLine("Press any key to quit");
Console.ReadKey();
Console.ReadKey();


