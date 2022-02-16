using TgNews.BL.Client;
using TgNews.BL.Repositories;
using TgNews.BL.Subscriptions;

namespace TgNews.BL.Services;

public class SubscriptionService
{
    private readonly SubscriptionRepository _db;
    private readonly TelegramBot _bot;

    public SubscriptionService(
        SubscriptionRepository db,
        TelegramBot bot
        )
    {
        _db = db;
        _bot = bot;
    }
    
    public void SaveLastReadMsgId(ITgSubscription subscription, int lastReadMsgId)
    {
        var entity = _db.Get(subscription.ChannelName);
        entity.TgLastReadMsgId = lastReadMsgId;
        _db.Update(entity);
    }

    public int GetLastReadMsgId(ITgSubscription subscription)
    {
        var entity = _db.Get(subscription.ChannelName);
        return entity.TgLastReadMsgId;
    }

    public async Task<long> GetTelegramSubscriptionChannelId(ITgSubscription subscription)
    {
        var entity = _db.Get(subscription.ChannelName);

        var cachedId = entity.TgChannelId;
        if (cachedId != default)
        {
            return cachedId;
        }

        var id = await _bot.GetChannelId(subscription.ChannelName);
        entity.TgChannelId = id;
        _db.Update(entity);

        return id;
    }
}