using TL;

namespace TgNews.BL.Client;

public class Telegram : IDisposable
{
    private WTelegram.Client? _telegram;

    public Telegram()
    {
        
    }

    public Task Init(TgNewsConfiguration cfg)
    {
        _telegram = new WTelegram.Client(cfg.TgConfig);
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
        _telegram?.Dispose();
    }
}