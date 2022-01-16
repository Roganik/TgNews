using TL;

namespace TgNews.BL.Subscriptions;

public class IfNews : ITgSubscription
{
    public string ChannelName => "if_market_news";
    public bool MarkAsReadAutomatically => true;

    public bool IsMessageInteresting(Message message)
    {
        var text = message.message;
        
        var stopWords = new List<string>
        {
            "Главные движения на рынк",
        };

        if (stopWords.Any(word => text.Contains(word)))
        {
            return false;
        }
        
        var interestingHashtags = new List<string>
        {
            "#AGRO",
            "#GMKN",
            "#OGKB",
            "#LSNG",
        };
        
        if (interestingHashtags.Any(word => text.Contains(word)))
        {
            return true;
        }

        return false;
    }
}