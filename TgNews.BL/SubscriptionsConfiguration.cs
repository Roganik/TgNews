using TgNews.BL.Subscriptions;

namespace TgNews.BL;

public class SubscriptionsConfiguration
{
    public SubscriptionCfg[] Subscriptions { get; set; }
}

public class SubscriptionCfg 
{
    public string ChannelName { get; set; }
    
    public bool MarkAsReadAutomatically { get; set; }

    public List<string> StopWords { get; set; }
    
    public List<string> InterestingWords { get; set; }
    
    public DefaultSubscriptionBehaviour DefaultBehaviour { get; set; }
}