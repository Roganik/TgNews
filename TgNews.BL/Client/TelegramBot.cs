using TL;

namespace TgNews.BL.Client;

public class TelegramBot : IDisposable
{
    private WTelegram.Client? _telegramBot;

    public TelegramBot()
    {
        
    }

    public Task Init(TgNewsConfiguration cfg)
    {
        _telegramBot = new WTelegram.Client(cfg.TgBotConfig);
        return _telegramBot.LoginBotIfNeeded();
    }
    
    public async Task ForwardMessages(int[] msgIds, string fromChat, string toChat = "roganik_wunsh")
    {
        var fromChatPeer = await _telegramBot.Contacts_ResolveUsername(fromChat);
        var toChatPeer = await _telegramBot.Contacts_ResolveUsername(toChat);
        var randomIds = msgIds.Select(x => WTelegram.Helpers.RandomLong()).ToArray(); // same size as an input array
        await _telegramBot.Messages_ForwardMessages(fromChatPeer, msgIds, randomIds, toChatPeer);
    }

    public void Dispose()
    {
        _telegramBot?.Dispose();
    }
}