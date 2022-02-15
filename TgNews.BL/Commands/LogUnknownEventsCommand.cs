using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TgNews.BL.Client;

namespace TgNews.BL.Commands;

public class LogUnknownEventsCommand
{
    private readonly Telegram _tg;
    private readonly ILogger _logger;

    public LogUnknownEventsCommand(
        Client.Telegram tg,
        ILogger logger)
    {
        _tg = tg;
        _logger = logger;
    }

    public async Task Init()
    {
        _tg.Events.OnUpdate += (update) =>
        {
            var updateType = update.GetType();
            var serializerOpts = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            var updateJson = JsonConvert.SerializeObject(update, updateType, serializerOpts);
            _logger.Log(LogLevel.Debug ,$"Got unknown update: {updateType.Name,25} Body: {Environment.NewLine}{updateJson}");
        };
    }
}