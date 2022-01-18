using TL;

namespace TgNews.BL.Subscriptions;

public class GenericSubscription : ITgSubscription
{
    public GenericSubscription(SubscriptionConfiguration cfg)
    {
        if (string.IsNullOrEmpty(cfg.ChannelName))
        {
            throw new ArgumentException("Unable to create GenericSubscription when channel name is not set");
        }
        
        this.ChannelName = cfg.ChannelName;
        this.InterestingWords = cfg.InterestingWords;
        this.StopWords = cfg.StopWords;
        this.MarkAsReadAutomatically = cfg.MarkAsReadAutomatically;
    }
    
    public string ChannelName { get; init; }
    public bool MarkAsReadAutomatically { get; init; }
    
    public DefaultSubscriptionBehaviour DefaultSubscriptionBehaviour { get; set; }
    public List<string> StopWords { get; init; }
    public List<string> InterestingWords { get; init; }
    
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

        if (this.DefaultSubscriptionBehaviour == DefaultSubscriptionBehaviour.ForwardMessage)
        {
            return true;
        }

        if (this.DefaultSubscriptionBehaviour == DefaultSubscriptionBehaviour.SkipMessage)
        {
            return false;
        }

        throw new NotImplementedException("Uh oh, seems like we in the impossible code path. Need to debug!");
    }
}

public enum DefaultSubscriptionBehaviour
{
    ForwardMessage,
    SkipMessage,
}