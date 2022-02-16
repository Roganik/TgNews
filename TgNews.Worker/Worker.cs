using TgNews.BL;
using TgNews.BL.Client;
using TgNews.BL.Repositories;
using TgNews.BL.Services;
using TgNews.BL.TgEventHandlers;

namespace TgNews.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly int _sleepSeconds;
    private readonly ForwardInterestingPostsFromEventsCommand _interestingPostsJob;
    private readonly UnknownEventHandler _unknownEventsHandler;
    private readonly Telegram _tg;
    private readonly MarkSubscriptionsAsReadEventHandler _markAsReadJob;
    private readonly TelegramBot _bot;

    public Worker(ILoggerFactory loggerFactory, IConfiguration icfg, TgSubscriptionsProvider subscriptions)
    {
        _logger = loggerFactory.CreateLogger<Worker>();
        
        var workerCfg = new WorkerConfiguration(icfg);
        var blCfg = new TgNewsConfiguration(icfg);
        
        _sleepSeconds = workerCfg.ForwarderCooldownSeconds;

        var tgLogger = loggerFactory.CreateLogger<Telegram>();
        _tg = new Telegram(blCfg, tgLogger);
        _bot = new TelegramBot(blCfg);
        var subscriptionRepo = new SubscriptionRepository(blCfg);
        var service = new SubscriptionService(subscriptionRepo, _bot);
        var eventsRepo = new EventRepository(blCfg);


        _interestingPostsJob = new ForwardInterestingPostsFromEventsCommand(_bot, blCfg, service, subscriptions, loggerFactory);
        _markAsReadJob = new MarkSubscriptionsAsReadEventHandler(_tg, service, subscriptions, loggerFactory);
        _unknownEventsHandler = new UnknownEventHandler(eventsRepo, loggerFactory);

#if !DEBUG
        var wTelegramLogger = loggerFactory.CreateLogger("WTelegram");
        WTelegram.Helpers.Log = (lvl, str) => wTelegramLogger.Log((LogLevel)lvl, str);
#endif
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);

        await _bot.Init();

        _unknownEventsHandler.Subscribe(_tg);
        _markAsReadJob.Subscribe(_tg);
        _interestingPostsJob.Subscribe(_tg);

        await _tg.Init();
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _interestingPostsJob.Execute();
            await _markAsReadJob.Execute();
            await Task.Delay(_sleepSeconds * 1000, stoppingToken);
        }
    }
}