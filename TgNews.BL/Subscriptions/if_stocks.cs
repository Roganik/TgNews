using TL;

namespace TgNews.BL.Subscriptions;

public class IfStocks : ITgSubscription
{
    public string ChannelName => "if_stocks";
    public bool MarkAsReadAutomatically => true;

    public bool IsMessageInteresting(Message message)
    {
        var text = message.message;
        
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