using TgNews.BL.Client;
using TgNews.BL.Subscriptions;

namespace TgNews.BL.Commands;

public class ForwardInterestingPostsCommand
{
    private readonly Telegram _telegram;
    private readonly TelegramBot _telegramBot;
    private readonly DbStorage _db;
    private readonly TgNewsConfiguration _cfg;
    private readonly List<ITgSubscription> _subscriptions;

    public ForwardInterestingPostsCommand(Telegram telegram, TelegramBot telegramBot, DbStorage db, TgNewsConfiguration cfg)
    {
        _telegram = telegram;
        _telegramBot = telegramBot;
        _db = db;
        _cfg = cfg;

        _subscriptions = new List<ITgSubscription>
        {
            new Bitkogan(),
            new GPBInvestments(),
        };
    }

    public async Task Execute()
    {
        foreach (var subscription in _subscriptions)
        {
            var lastMsgId = _db.ReadKey<int>(subscription.ChannelName);
            var isFirstRun = lastMsgId == 0;

            var resp = await _telegram.GetMessages(subscription.ChannelName, minMessageId: lastMsgId);
            if (!resp.Messages.Any())
            {
                continue;
            }
            
            lastMsgId = resp.Messages.Max(m => m.id);
            if (subscription.MarkAsReadAutomatically)
            {
                await _telegram.MarkChannelAsRead(subscription.ChannelName, lastMsgId);
            }

            var interestingMessagesIds = resp.Messages
                .Where(message => subscription.IsMessageInteresting(message))
                .Select(message => message.id)
                .OrderBy(id => id) // to make forwarding ordering
                .ToArray();

            if (interestingMessagesIds.Length == 0)
            {
                _db.SetKey(subscription.ChannelName, lastMsgId);
                continue;
            }
            
            if (isFirstRun)
            {
                interestingMessagesIds = interestingMessagesIds.TakeLast(2).ToArray();
            }

            await _telegramBot.ForwardMessages(interestingMessagesIds, subscription.ChannelName, _cfg.TgBotForwardToChannel);
            _db.SetKey(subscription.ChannelName, lastMsgId);
        }
    }
}