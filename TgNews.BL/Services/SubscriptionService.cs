using TgNews.BL.Client;
using TgNews.BL.Subscriptions;

namespace TgNews.BL.Services;

public class SubscriptionService
{
    private readonly DbStorage _db;

    public SubscriptionService(DbStorage db)
    {
        _db = db;
    }
    
    public void SaveLastProcessedMsgId(ITgSubscription subscription, int lastReadMsgId)
    {
        _db.SetKey(subscription.ChannelName + "_tgLastReadMsgId", lastReadMsgId);
    }

    public int GetLastProcessedMsgId(ITgSubscription subscription)
    {
        return _db.ReadKey<int>(subscription.ChannelName + "_tgLastReadMsgId");
    }

    public void SaveTelegramSubscriptionChannelId(ITgSubscription subscription, long telegramChannelId)
    {
        _db.SetKey(subscription.ChannelName + "_tgChannelId", telegramChannelId);
    }
    
    public long GetTelegramSubscriptionChannelId(ITgSubscription subscription)
    {
       return _db.ReadKey<long>(subscription.ChannelName + "_tgChannelId");
    }
}