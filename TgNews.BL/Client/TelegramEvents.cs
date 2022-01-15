using TL;

namespace TgNews.BL.Client;

public class TelegramEvents
{
    internal TelegramEvents()
    {

    }

    internal void Subscription(IObject arg)
    {
        if (arg is not UpdatesBase updates)
        {
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
                default:
                    OnUpdate?.Invoke(abstractUpdate);
                    break;
            }
        }
    }

    public event Action<UpdateEditChannelMessage, Message>? OnUpdateEditChannelMessage;
    public event Action<UpdateEditChannelMessage, MessageService>? OnUpdateEditChannelMessageService;
    public event Action<UpdateMessagePoll>? OnUpdateMessagePoll;
    public event Action<UpdateUserStatus>? OnUpdateUserStatus;

    /// <summary> Handler for untyped messages </summary>
    public event Action<Update>? OnUpdate;

}