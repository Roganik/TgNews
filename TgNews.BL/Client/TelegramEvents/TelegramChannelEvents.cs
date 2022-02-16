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
                OnEditMessage?.Invoke(update, msg);
                return true;

            case UpdateEditChannelMessage {message: MessageService msg} update:
                OnEditMessageService?.Invoke(update, msg);
                return true;

            case UpdateNewChannelMessage {message: Message msg} update:
                OnNewMessage?.Invoke(update, msg);
                return true;

            case UpdateNewChannelMessage {message: MessageService msg} update:
                OnNewMessageService?.Invoke(update, msg);
                return true;

            case UpdateChannelUserTyping update:
                OnaUserTyping?.Invoke(update);
                return true;

            case UpdateChannelMessageViews update:
                OnUpdateMessageViews?.Invoke(update);
                return true;

            case UpdateDeleteChannelMessages update:
                OnDeleteMessages?.Invoke(update);
                return true;

            default:
                return false;
        }
    }


    public event Action<UpdateEditChannelMessage, Message>? OnEditMessage;
    public event Action<UpdateEditChannelMessage, MessageService>? OnEditMessageService;
    public event Action<UpdateNewChannelMessage, Message>? OnNewMessage;
    public event Action<UpdateNewChannelMessage, MessageService>? OnNewMessageService;
    public event Action<UpdateChannelUserTyping>? OnaUserTyping;
    public event Action<UpdateChannelMessageViews>? OnUpdateMessageViews;
    public event Action<UpdateDeleteChannelMessages>? OnDeleteMessages;

}