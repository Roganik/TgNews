using TgNews.BL.Client;
using TgNews.BL.Subscriptions;

namespace TgNews.BL.Services;

public class LastProcessedMsgIdForSubscriptionService
{
    private readonly DbStorage _db;

    public LastProcessedMsgIdForSubscriptionService(DbStorage db)
    {
        _db = db;
    }

    public void Save(ITgSubscription subscription, int lastReadMsgId)
    {
        _db.SetKey(subscription.ChannelName, lastReadMsgId);
    }

    public int Get(ITgSubscription subscription)
    {
        return _db.ReadKey<int>(subscription.ChannelName);
    }
}