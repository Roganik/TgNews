using TL;

namespace TgNews.BL.Subscriptions;

public class GPBInvestments : ITgSubscription
{
    public string ChannelName => "gpb_investments";
    public bool MarkAsReadAutomatically => true;

    public bool IsMessageInteresting(Message message)
    {
        var text = message.message;
        var stopWords = new List<string>
        {
            "#gpbi_news",
            "#gpbi_corp",
            "Shanghai Composite:", // вторая часть поста про дневные новости
            "EUR/RUB:", // третья часть поста про дневные новости
        };

        if (stopWords.Any(word => text.Contains(word)))
        {
            return false;
        }

        return true;
    }
}