using TgNews.BL;
using TgNews.BL.Client;
using TgNews.BL.Commands;

namespace TgNews.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly int _sleepSeconds;
    private readonly ForwardInterestingPostsFromEventsCommand _job;

    public Worker(ILoggerFactory loggerFactory, IConfiguration icfg, TgSubscriptionsProvider subscriptions)
    {
        _logger = loggerFactory.CreateLogger<Worker>();
        
        var workerCfg = new WorkerConfiguration(icfg);
        var blCfg = new TgNewsConfiguration(icfg);
        
        _sleepSeconds = workerCfg.ForwarderCooldownSeconds;

        var tgLogger = loggerFactory.CreateLogger<Telegram>();
        var tg = new Telegram(blCfg, tgLogger);
        var bot = new TelegramBot(blCfg);
        var db = new DbStorage(blCfg);

        var newPostsLogger = loggerFactory.CreateLogger("ForwardNewPosts");
        _job = new ForwardInterestingPostsFromEventsCommand(tg, bot, db, blCfg, subscriptions, newPostsLogger);

#if !DEBUG
        var wTelegramLogger = loggerFactory.CreateLogger("WTelegram");
        WTelegram.Helpers.Log = (lvl, str) => wTelegramLogger.Log((LogLevel)lvl, str);
#endif
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);
        
        await _job.Init();
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _job.Execute();
            await Task.Delay(_sleepSeconds * 1000, stoppingToken);
        }
    }
}