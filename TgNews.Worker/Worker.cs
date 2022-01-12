using TgNews.BL;

namespace TgNews.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private Forwarder? _forwarder;
    private int _sleepSeconds = 1000 * 60;
    private readonly TgNewsConfiguration _cfg;

    public Worker(ILogger<Worker> logger, IConfiguration icfg)
    {
        _cfg = new TgNewsConfiguration(icfg);
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);
        
        WTelegram.Helpers.Log = (lvl, str) => _logger.Log((LogLevel)lvl, str);
        
        var db = new TgNews.BL.Client.DbStorage(_cfg);
        var tg = new TgNews.BL.Client.Telegram();
        var tgBot = new TgNews.BL.Client.TelegramBot();

        await tg.Init(_cfg);
        await tgBot.Init(_cfg);

        _forwarder = new Forwarder(tg, tgBot, db);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _forwarder.Execute();
            await Task.Delay(_sleepSeconds, stoppingToken);
        }
    }
}