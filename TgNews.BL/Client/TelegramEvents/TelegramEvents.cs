using TL;

namespace TgNews.BL.Client.TelegramEvents;

public class TelegramEvents
{
    internal TelegramEvents()
    {
    }

    internal void Subscription(IObject arg)
    {
        if (arg is ReactorError re)
        {
            OnReactorError?.Invoke(re);
            return;
        }

        if (arg is TL.Updates_State us)
        {
            OnUpdateState?.Invoke(us);
            return;
        }

        if (arg is not UpdatesBase updates)
        {
            OnUnknownEvent?.Invoke(arg);
            return;
        }
        
        foreach (var abstractUpdate in updates.UpdateList)
        {
            switch (abstractUpdate)
            {
                case UpdateEditChannelMessage {message: Message msg} update:
                    OnUpdateEditChannelMessage?.Invoke(update, msg);
                    break;
                case UpdateEditChannelMessage {message: MessageService msg} update:
                    OnUpdateEditChannelMessageService?.Invoke(update, msg);
                    break;
                case UpdateUserStatus update:
                    OnUpdateUserStatus?.Invoke(update);
                    break;
                case UpdateMessagePoll update:
                    OnUpdateMessagePoll?.Invoke(update); 
                    break;
                case UpdateNewChannelMessage {message: Message msg} update:
                    OnUpdateNewChannelMessage?.Invoke(update, msg);
                    break;
                case UpdateUserTyping update:
                    OnUpdateUserTyping?.Invoke(update);
                    break;
                case UpdateChannelUserTyping update:
                    OnUpdateChannelUserTyping?.Invoke(update);
                    break;
                case UpdateChannelMessageViews update:
                    OnUpdateChannelMessageViews?.Invoke(update);
                    break;
                case UpdateNewMessage {message: Message msg} update:
                    OnUpdateNewMessage?.Invoke(update, msg);
                    break;
                default:
                    OnUpdate?.Invoke(abstractUpdate);
                    break;
            }
        }
    }

    public event Action<IObject>? OnUnknownEvent;
    public event Action<Updates_State>? OnUpdateState;
    public event Action<ReactorError>? OnReactorError;

    public event Action<UpdateEditChannelMessage, Message>? OnUpdateEditChannelMessage;
    public event Action<UpdateEditChannelMessage, MessageService>? OnUpdateEditChannelMessageService;
    public event Action<UpdateMessagePoll>? OnUpdateMessagePoll;
    public event Action<UpdateUserStatus>? OnUpdateUserStatus;
    public event Action<UpdateNewChannelMessage, Message>? OnUpdateNewChannelMessage;
    public event Action<UpdateUserTyping> OnUpdateUserTyping;
    public event Action<UpdateChannelUserTyping> OnUpdateChannelUserTyping;
    public event Action<UpdateChannelMessageViews> OnUpdateChannelMessageViews;
    public event Action<UpdateNewMessage, Message> OnUpdateNewMessage;

    /// <summary> Handler for untyped messages </summary>
    public event Action<Update>? OnUpdate;

}