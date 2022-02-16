using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TgNews.BL.Client;
using TgNews.BL.Repositories;

namespace TgNews.BL.TgEventHandlers;

public class UnknownEventHandler
{
    private readonly EventRepository _repo;
    private readonly ILogger _logger;
    private int _unknownEventsSavedCounter = 0;

    public UnknownEventHandler(
        EventRepository repo,
        ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _logger = loggerFactory.CreateLogger("UnknownEvents");
    }

    public void Subscribe(Telegram tg)
    {
        tg.Events.OnUpdate += (update) =>
        {
            var updateType = update.GetType();
            var serializerOpts = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            var updateJson = JsonConvert.SerializeObject(update, updateType, serializerOpts);

            _repo.Insert(updateType.Name, DateTime.Now, updateJson);

            var unknownEventCount = Interlocked.Increment(ref _unknownEventsSavedCounter);
            if (unknownEventCount % 25 == 0)
            {
                _logger.Log(LogLevel.Information ,$"Saved: {unknownEventCount,4} unknown events to DB during this session");
            }
        };
    }
}