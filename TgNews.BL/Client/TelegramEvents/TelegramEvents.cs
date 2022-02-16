using TL;

namespace TgNews.BL.Client.TelegramEvents;

public class TelegramEvents
{
    public readonly TelegramChannelEvents Channel = new ();

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
            if (Channel.HandleUpdate(abstractUpdate))
            {
                continue;
            }

            switch (abstractUpdate)
            {
                case UpdateUserStatus update:
                    OnUpdateUserStatus?.Invoke(update);
                    break;
                case UpdateMessagePoll update:
                    OnUpdateMessagePoll?.Invoke(update); 
                    break;
                case UpdateUserTyping update:
                    OnUpdateUserTyping?.Invoke(update);
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

    public event Action<UpdateMessagePoll>? OnUpdateMessagePoll;
    public event Action<UpdateUserStatus>? OnUpdateUserStatus;
    public event Action<UpdateUserTyping>? OnUpdateUserTyping;
    public event Action<UpdateNewMessage, Message>? OnUpdateNewMessage;

    /// <summary> Handler for untyped messages </summary>
    public event Action<Update>? OnUpdate;

}