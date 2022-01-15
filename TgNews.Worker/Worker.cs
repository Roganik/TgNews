using TgNews.BL;
using TgNews.BL.Client;
using TgNews.BL.Commands;

namespace TgNews.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly int _sleepSeconds;
    private readonly ForwardInterestingPostsFromEventsCommand _job;

    public Worker(ILogger<Worker> logger, IConfiguration icfg)
    {
        _logger = logger;
        
        var workerCfg = new WorkerConfiguration(icfg);
        var blCfg = new TgNewsConfiguration(icfg);
        
        _sleepSeconds = workerCfg.ForwarderCooldownSeconds;
        
        var tg = new Telegram(blCfg);
        var bot = new TelegramBot(blCfg);
        var db = new DbStorage(blCfg);

        _job = new ForwardInterestingPostsFromEventsCommand(tg, bot, db, blCfg, new TgSubscriptionsProvider(), logger);
        
#if !DEBUG
        WTelegram.Helpers.Log = (lvl, str) => _logger.Log((LogLevel)lvl, str);
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