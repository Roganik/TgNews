namespace TgNews.BL.Subscriptions;

public interface ITgSubscription
{ 
    string ChannelName { get; }
    
    bool MarkAsReadAutomatically { get; }

    bool IsMessageInteresting(TL.Message message);
    
}