using TgNews.BL.Subscriptions;

namespace TgNews.BL;

public class TgSubscriptionsProvider
{
    public List<ITgSubscription> GetAll()
    {
        return new List<ITgSubscription>
        {
            new Bitkogan(),
            new GPBInvestments(),
        };
    }
}