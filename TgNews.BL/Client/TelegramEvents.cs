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
                    OnUpdateEditChannelMessage(update, msg);
                    break;
                case UpdateEditChannelMessage {message: MessageService msg} update:
                    OnUpdateEditChannelMessageService(update, msg);
                    break;
                case UpdateUserStatus update:
                    OnUpdateUserStatus(update);
                    break;
                case UpdateMessagePoll update:
                    OnUpdateMessagePoll(update); 
                    break;
                default:
                    OnUpdate(abstractUpdate);
                    break;
            }
        }
    }

    public Action<UpdateEditChannelMessage, Message> OnUpdateEditChannelMessage { get; set; } = (_, _) => { };
    public Action<UpdateEditChannelMessage, MessageService> OnUpdateEditChannelMessageService { get; set; } = (_, _) => { };
    public Action<UpdateMessagePoll> OnUpdateMessagePoll { get; set; } = (_) => { };
    public Action<UpdateUserStatus> OnUpdateUserStatus { get; set; } = (_) => { };

    /// <summary> Handler for untyped messages </summary>
    public Action<Update> OnUpdate { get; set; } = (_) => { };

}