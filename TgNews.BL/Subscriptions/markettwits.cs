using TL;

namespace TgNews.BL.Subscriptions;

public class MarketTwits : ITgSubscription
{
    public string ChannelName => "markettwits";
    public bool MarkAsReadAutomatically => true;

    public bool IsMessageInteresting(Message message)
    {
        var text = message.message;
        
        var stopWords = new List<string>
        {
            "КАЛЕНДАРЬ НА СЕГОДНЯ",
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