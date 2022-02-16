using Microsoft.Extensions.Logging;
using TgNews.BL.Client;
using TgNews.BL.Subscriptions;

namespace TgNews.BL.Services;

public class SubscriptionService
{
    private readonly DbStorage _db;
    private readonly TelegramBot _bot;

    public SubscriptionService(
        DbStorage db,
        TelegramBot bot
        )
    {
        _db = db;
        _bot = bot;
    }
    
    public void SaveLastProcessedMsgId(ITgSubscription subscription, int lastReadMsgId)
    {
        _db.SetKey(subscription.ChannelName + "_tgLastReadMsgId", lastReadMsgId);
    }

    public int GetLastProcessedMsgId(ITgSubscription subscription)
    {
        return _db.ReadKey<int>(subscription.ChannelName + "_tgLastReadMsgId");
    }

    public async Task<long> GetTelegramSubscriptionChannelId(ITgSubscription subscription)
    {
        var cachedId = _db.ReadKey<long>(subscription.ChannelName + "_tgChannelId");
        if (cachedId != default)
        {
            return cachedId;
        }

        var id = await _bot.GetChannelId(subscription.ChannelName);
        _db.SetKey(subscription.ChannelName + "_tgChannelId", id);

        return id;
    }
}