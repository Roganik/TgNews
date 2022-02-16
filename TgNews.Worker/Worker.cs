using TgNews.BL;
using TgNews.BL.Client;
using TgNews.BL.Commands;
using TgNews.BL.TgEventHandlers;

namespace TgNews.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly int _sleepSeconds;
    private readonly ForwardInterestingPostsFromEventsCommand _job;
    private readonly UnknownEventHandler _unknownEventsHandler;
    private readonly Telegram _tg;

    public Worker(ILoggerFactory loggerFactory, IConfiguration icfg, TgSubscriptionsProvider subscriptions)
    {
        _logger = loggerFactory.CreateLogger<Worker>();
        
        var workerCfg = new WorkerConfiguration(icfg);
        var blCfg = new TgNewsConfiguration(icfg);
        
        _sleepSeconds = workerCfg.ForwarderCooldownSeconds;

        var tgLogger = loggerFactory.CreateLogger<Telegram>();
        _tg = new Telegram(blCfg, tgLogger);
        var bot = new TelegramBot(blCfg);
        var db = new DbStorage(blCfg);

        var newPostsLogger = loggerFactory.CreateLogger("ForwardNewPosts");
        _job = new ForwardInterestingPostsFromEventsCommand(_tg, bot, db, blCfg, subscriptions, newPostsLogger);
        _unknownEventsHandler = new UnknownEventHandler(loggerFactory);

#if !DEBUG
        var wTelegramLogger = loggerFactory.CreateLogger("WTelegram");
        WTelegram.Helpers.Log = (lvl, str) => wTelegramLogger.Log((LogLevel)lvl, str);
#endif
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);

        _unknownEventsHandler.Subscribe(_tg);
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