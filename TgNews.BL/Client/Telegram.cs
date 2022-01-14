using TL;
using WTelegram;

namespace TgNews.BL.Client;

public class Telegram : IDisposable
{
    private readonly WTelegram.Client _telegram;

    public Telegram(TgNewsConfiguration cfg)
    {
        _telegram = new WTelegram.Client(cfg.TgConfig);

        if (!string.IsNullOrEmpty(cfg.TgClientFloodAutoRetrySecondsThreshold) && int.TryParse(cfg.TgClientFloodAutoRetrySecondsThreshold, out var seconds))
        {
            _telegram.FloodRetryThreshold = seconds;
        }
    }

    public Task Init(TypedUpdates tgEventsSubscription = null)
    {
        if (tgEventsSubscription != null)
        {
            _telegram.Update += tgEventsSubscription.Subscription;
        }
        return _telegram.LoginUserIfNeeded();
    }

    public async Task<(List<Message> Messages, Contacts_ResolvedPeer Peer)> GetMessages(string channelName, int minMessageId = 0)
    {
        var resolved = await _telegram.Contacts_ResolveUsername(channelName);
        if (resolved.UserOrChat is Channel channel)
        {
            var offset = 0;
            var limit = 200;
            var messagesResponce = await _telegram.Messages_GetHistory(
                channel, 0, default, offset, limit, 0, minMessageId, 0);

            var messages = messagesResponce.Messages
                .Select(m => m as Message) // filter out TL.MessageService
                .Where(m => m != null)
                .ToList();
            
            return (messages, resolved);
        }

        throw new InvalidCastException("Only channel messages getting is implemented");
    }

    public async Task MarkChannelAsRead(string channelName, int maxReadMsgId)
    {
        var resolved = await _telegram.Contacts_ResolveUsername(channelName);
        if (resolved.UserOrChat is Channel channel)
        {
            await _telegram.Channels_ReadHistory(channel, maxReadMsgId);
            return;
        }

        throw new InvalidCastException("Only channel messages getting is implemented");
    }

    public void Dispose()
    {
        _telegram.Dispose();
    }
}