using TgNews.BL.Subscriptions;

namespace TgNews.BL;

public class SubscriptionsConfigurationSection
{
    public SubscriptionConfiguration[] Subscriptions { get; set; }
}

public class SubscriptionConfiguration 
{
    public string ChannelName { get; set; }
    
    public bool MarkAsReadAutomatically { get; set; }

    public List<string>? StopWords { get; set; }
    
    public List<string>? InterestingWords { get; set; }
    
    public DefaultSubscriptionBehaviour DefaultBehaviour { get; set; }
}