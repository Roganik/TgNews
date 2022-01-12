using TL;

namespace TgNews.BL.Subscriptions;

public class Bitkogan : ITgSubscription
{
    public string ChannelName => "bitkogan";
    public bool MarkAsReadAutomatically => true;
    
    public bool IsMessageInteresting(Message message)
    {
        var text = message.message;
        var stopWords = new List<string>
        {
            "#видео",
            "#реклама",
            "Новое видео в YouTube",
        };

        if (stopWords.Any(word => text.Contains(word)))
        {
            return false;
        }
        
        return true;
    }
}