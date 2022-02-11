using Microsoft.Extensions.Logging;
using TL;

namespace TgNews.BL.Client;

public class Telegram : IDisposable
{
    private readonly TgNewsConfiguration _cfg;
    private WTelegram.Client _telegram;
    public TelegramEvents Events { get; }
    public TelegramCache Cache { get; }

    public Telegram(TgNewsConfiguration cfg, ILogger<Telegram> logger)
    {
        _cfg = cfg;
        this.Events = new TelegramEvents();
        this.Cache = new TelegramCache();
        _telegram = CreateNewTelegramInstance();

        Events.OnReactorError += (re) =>
        {
            logger.LogCritical("Received ReactorError from telegram client. Will Try to re-create WTelegram.Client");
            _telegram.Dispose();

            logger.LogCritical("Creating new WTelegram.Client instance");
            _telegram = CreateNewTelegramInstance();

            logger.LogCritical("Created new WTelegram.Client instance. Logging in");
            Init();
        };

        Events.OnUnknownEvent += (arg) =>
        {
            logger.LogWarning($"Unexpected Telegram event received. Type= {arg.GetType()}");
        };
    }

    private WTelegram.Client CreateNewTelegramInstance()
    {
        var telegram = new WTelegram.Client(_cfg.TgConfig);
        telegram.Update += Events.Subscription;
        telegram.Update += Cache.Subscription;

        if (!string.IsNullOrEmpty(_cfg.TgClientFloodAutoRetrySecondsThreshold) && int.TryParse(_cfg.TgClientFloodAutoRetrySecondsThreshold, out var seconds))
        {
            telegram.FloodRetryThreshold = seconds;
        }

        return telegram;
    }

    public Task Init()
    {
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

            return (messages, resolved)!;
        }

        throw new InvalidCastException("Only channel messages getting is implemented");
    }

    public async Task MarkChannelAsRead(string channelName, int maxReadMsgId)
    {
        // todo: probably it's possible to implement some sort of caching here
        // todo: try to use cached channel info instead of resolving username below
        var resolved = await _telegram.Contacts_ResolveUsername(channelName);
        if (resolved.UserOrChat is Channel channel)
        {
            await _telegram.Channels_ReadHistory(channel, maxReadMsgId);
            return;
        }

        throw new InvalidCastException("Only channel messages reading is implemented");
    }

    public void Dispose()
    {
        _telegram.Dispose();
    }
}