using TL;

namespace TgNews.BL.Client;

public class TelegramBot : IDisposable
{
    private readonly WTelegram.Client _telegramBot;

    public TelegramBot(TgNewsConfiguration cfg)
    {
        _telegramBot = new WTelegram.Client(cfg.TgBotConfig);
    }

    public Task Init()
    {
        return _telegramBot.LoginBotIfNeeded();
    }
    
    public async Task ForwardMessages(int[] msgIds, string fromChat, string toChat)
    {
        var fromChatPeer = await _telegramBot.Contacts_ResolveUsername(fromChat);
        var toChatPeer = await _telegramBot.Contacts_ResolveUsername(toChat);
        var randomIds = msgIds.Select(x => WTelegram.Helpers.RandomLong()).ToArray(); // same size as an input array
        await _telegramBot.Messages_ForwardMessages(fromChatPeer, msgIds, randomIds, toChatPeer);
    }

    public async Task<long> GetChannelId(string channelName)
    {
        var channel = await _telegramBot.Contacts_ResolveUsername(channelName);
        return channel.Chat.ID;
    }

    public void Dispose()
    {
        _telegramBot?.Dispose();
    }
}