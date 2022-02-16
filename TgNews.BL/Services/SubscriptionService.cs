using TgNews.BL.Client;
using TgNews.BL.Repositories;
using TgNews.BL.Subscriptions;

namespace TgNews.BL.Services;

public class SubscriptionService
{
    private readonly KeyValueRepository _db;
    private readonly TelegramBot _bot;

    public SubscriptionService(
        KeyValueRepository db,
        TelegramBot bot
        )
    {
        _db = db;
        _bot = bot;
    }
    
    public void SaveLastReadMsgId(ITgSubscription subscription, int lastReadMsgId)
    {
        _db.SetKey(subscription.ChannelName + "_tgLastReadMsgId", lastReadMsgId);
    }

    public int GetLastReadMsgId(ITgSubscription subscription)
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