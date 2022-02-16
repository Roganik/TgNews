using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TgNews.BL.Client;

namespace TgNews.BL.TgEventHandlers;

public class UnknownEventHandler
{
    private readonly ILogger _logger;

    public UnknownEventHandler(
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("UnknownEvents");
    }

    public void Subscribe(Telegram tg)
    {
        tg.Events.OnUpdate += (update) =>
        {
            var updateType = update.GetType();
            var serializerOpts = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            var updateJson = JsonConvert.SerializeObject(update, updateType, serializerOpts);
            _logger.Log(LogLevel.Debug ,$"Got unknown update: {updateType.Name,25} Body: {Environment.NewLine}{updateJson}");
        };
    }
}