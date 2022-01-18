using TgNews.BL.Subscriptions;

namespace TgNews.BL;

public class TgSubscriptionsProvider
{
    private readonly SubscriptionsConfigurationSection _section;

    public TgSubscriptionsProvider(SubscriptionsConfigurationSection section)
    {
        _section = section;
    }

    public List<ITgSubscription> GetAll()
    {
        var result = new List<ITgSubscription>();
        foreach (var subscriptionCfg in _section.Subscriptions)
        {
            result.Add(new GenericSubscription(subscriptionCfg));
        }

        return result;
    }
}