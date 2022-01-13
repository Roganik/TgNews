using TgNews.BL;

namespace TgNews.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private Forwarder? _forwarder;
    private readonly int _sleepSeconds;
    private readonly TgNewsConfiguration _tgCfg;
    private readonly WorkerConfiguration _workerCfg;

    public Worker(ILogger<Worker> logger, IConfiguration icfg)
    {
        _tgCfg = new TgNewsConfiguration(icfg);
        _workerCfg = new WorkerConfiguration(icfg);
        _logger = logger;
        _sleepSeconds = _workerCfg.ForwarderCooldownSeconds;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);
        
        WTelegram.Helpers.Log = (lvl, str) => _logger.Log((LogLevel)lvl, str);
        
        var db = new TgNews.BL.Client.DbStorage(_tgCfg);
        var tg = new TgNews.BL.Client.Telegram(_tgCfg);
        var tgBot = new TgNews.BL.Client.TelegramBot(_tgCfg);

        await tg.Init();
        await tgBot.Init();

        _forwarder = new Forwarder(tg, tgBot, db);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _forwarder.Execute();
            await Task.Delay(_sleepSeconds * 1000, stoppingToken);
        }
    }
}