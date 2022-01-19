using TgNews.BL.Subscriptions;

namespace TgNews.BL;

public class TgSubscriptionsProvider
{
    private readonly SubscriptionsConfiguration _configuration;

    public TgSubscriptionsProvider(SubscriptionsConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<ITgSubscription> GetAll()
    {
        var result = new List<ITgSubscription>();
        foreach (var subscriptionCfg in _configuration.Subscriptions)
        {
            result.Add(new GenericSubscription(subscriptionCfg));
        }

        return result;
    }
}