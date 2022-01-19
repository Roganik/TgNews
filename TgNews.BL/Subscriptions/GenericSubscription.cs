using TL;

namespace TgNews.BL.Subscriptions;

public class GenericSubscription : ITgSubscription
{
    public GenericSubscription(SubscriptionCfg cfg)
    {
        if (string.IsNullOrEmpty(cfg.ChannelName))
        {
            throw new ArgumentException("Unable to create GenericSubscription when channel name is not set");
        }
        
        this.ChannelName = cfg.ChannelName;
        this.InterestingWords = cfg.InterestingWords;
        this.StopWords = cfg.StopWords;
        this.MarkAsReadAutomatically = cfg.MarkAsReadAutomatically;
        this.DefaultSubscriptionBehaviour = cfg.DefaultBehaviour;
    }
    
    public string ChannelName { get; }
    public bool MarkAsReadAutomatically { get; }
    public DefaultSubscriptionBehaviour DefaultSubscriptionBehaviour { get; }
    public List<string> StopWords { get; }
    public List<string> InterestingWords { get; }
    
    public bool IsMessageInteresting(Message message)
    {
        var text = message.message;
        
        var hasStopWord = StopWords?.Any(word => text.Contains(word, StringComparison.InvariantCultureIgnoreCase));
        if (hasStopWord == true)
        {
            return false;
        }
        
        var hasInterestingWord = InterestingWords?.Any(word => text.Contains(word, StringComparison.InvariantCultureIgnoreCase));
        if (hasInterestingWord == true)
        {
            return true;
        }

        return this.DefaultSubscriptionBehaviour switch
        {
            DefaultSubscriptionBehaviour.ForwardMessage => true,
            DefaultSubscriptionBehaviour.SkipMessage => false,
            _ => throw new NotImplementedException("Uh oh, seems like we in the impossible code path. Need to debug!")
        };
    }
}

public enum DefaultSubscriptionBehaviour
{
    ForwardMessage,
    SkipMessage,
}