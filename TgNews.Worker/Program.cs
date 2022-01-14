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

tg.Events.OnUpdate = (update) =>
{
    var type = update.GetType();
    var serializerOpts = new JsonSerializerSettings() {Formatting = Formatting.Indented};
    var json = JsonConvert.SerializeObject(update, type, serializerOpts);
    logger.LogWarning($"Untyped update received: {type.Name}");
    logger.LogInformation(json);
};

tg.Events.OnUpdateEditChannelMessage = (update, msg) =>
{
    // todo
};


await tg.Init();
await tgBot.Init();

Console.WriteLine("Press any key to quit");
Console.ReadKey();
Console.ReadKey();


