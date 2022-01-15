using TgNews.BL.Client;
using TgNews.BL.Services;
using TgNews.BL.Subscriptions;

namespace TgNews.BL.Commands;

public class ForwardInterestingPostsCommand
{
    private readonly Telegram _telegram;
    private readonly TelegramBot _telegramBot;
    private readonly TgNewsConfiguration _cfg;
    private readonly LastProcessedMsgIdForSubscriptionService _lastProcessedMsgIdService;

    public ForwardInterestingPostsCommand(Telegram telegram, TelegramBot telegramBot, DbStorage db, TgNewsConfiguration cfg)
    {
        _telegram = telegram;
        _telegramBot = telegramBot;
        _lastProcessedMsgIdService = new LastProcessedMsgIdForSubscriptionService(db);
        _cfg = cfg;
    }

    public async Task Execute(List<ITgSubscription> subscriptions)
    {
        foreach (var subscription in subscriptions)
        {
            var lastMsgId = _lastProcessedMsgIdService.Get(subscription);
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
                _lastProcessedMsgIdService.Save(subscription, lastMsgId);

                continue;
            }
            
            if (isFirstRun)
            {
                interestingMessagesIds = interestingMessagesIds.TakeLast(2).ToArray();
            }

            await _telegramBot.ForwardMessages(interestingMessagesIds, subscription.ChannelName, _cfg.TgBotForwardToChannel);
            _lastProcessedMsgIdService.Save(subscription, lastMsgId);
        }
    }
}