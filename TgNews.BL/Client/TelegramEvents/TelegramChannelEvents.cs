using TL;

namespace TgNews.BL.Client.TelegramEvents;

public class TelegramChannelEvents
{
    internal TelegramChannelEvents()
    {
    }

    internal bool HandleUpdate(Update abstractUpdate)
    {
        switch (abstractUpdate)
        {
            case UpdateEditChannelMessage {message: Message msg} update:
                OnUpdateEditChannelMessage?.Invoke(update, msg);
                return true;

            case UpdateEditChannelMessage {message: MessageService msg} update:
                OnUpdateEditChannelMessageService?.Invoke(update, msg);
                return true;

            case UpdateNewChannelMessage {message: Message msg} update:
                OnUpdateNewChannelMessage?.Invoke(update, msg);
                return true;

            case UpdateNewChannelMessage {message: MessageService msg} update:
                OnUpdateNewChannelMessageService?.Invoke(update, msg);
                return true;

            case UpdateChannelUserTyping update:
                OnUpdateChannelUserTyping?.Invoke(update);
                return true;

            case UpdateChannelMessageViews update:
                OnUpdateChannelMessageViews?.Invoke(update);
                return true;

            case UpdateDeleteChannelMessages update:
                OnUpdateDeleteChannelMessages?.Invoke(update);
                return true;

            default:
                return false;
        }
    }


    public event Action<UpdateEditChannelMessage, Message>? OnUpdateEditChannelMessage;
    public event Action<UpdateEditChannelMessage, MessageService>? OnUpdateEditChannelMessageService;
    public event Action<UpdateNewChannelMessage, Message>? OnUpdateNewChannelMessage;
    public event Action<UpdateNewChannelMessage, MessageService>? OnUpdateNewChannelMessageService;
    public event Action<UpdateChannelUserTyping>? OnUpdateChannelUserTyping;
    public event Action<UpdateChannelMessageViews>? OnUpdateChannelMessageViews;
    public event Action<UpdateDeleteChannelMessages>? OnUpdateDeleteChannelMessages;

}