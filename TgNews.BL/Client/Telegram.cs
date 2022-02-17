using Microsoft.Extensions.Logging;
using TL;

namespace TgNews.BL.Client;

public class Telegram : IDisposable
{
    private readonly TgNewsConfiguration _cfg;
    private readonly ILogger<Telegram> _logger;
    private WTelegram.Client _telegram;
    public TelegramEvents.TelegramEvents Events { get; }

    public Telegram(TgNewsConfiguration cfg, ILogger<Telegram> logger)
    {
        _cfg = cfg;
        _logger = logger;
        this.Events = new TelegramEvents.TelegramEvents();
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

        if (!string.IsNullOrEmpty(_cfg.TgClientFloodAutoRetrySecondsThreshold) && int.TryParse(_cfg.TgClientFloodAutoRetrySecondsThreshold, out var seconds))
        {
            telegram.FloodRetryThreshold = seconds;
            telegram.CollectAccessHash = true;
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

    public async Task MarkChannelAsRead(string channelName, long channelId, int maxReadMsgId)
    {
        var accessHash = _telegram.GetAccessHashFor<Channel>(channelId);
        if (accessHash == default)
        {
            _logger.LogInformation($"Didn't found access hash for {channelName} (id: {channelId}) in cache, requesting it from tg manually");
            var resolved = await _telegram.Contacts_ResolveUsername(channelName);
            var channel = resolved.UserOrChat as Channel;
            accessHash = channel?.access_hash ?? default;
        }

        if (accessHash == default)
        {
            throw new InvalidCastException("unable to resolve accessHash. Note: Only channel messages reading is implemented.");
        }

        var inputChannel = new InputChannel()
        {
            access_hash = accessHash,
            channel_id = channelId,
        };

        await _telegram.Channels_ReadHistory(inputChannel, maxReadMsgId);
    }

    public void Dispose()
    {
        _telegram.Dispose();
    }
}