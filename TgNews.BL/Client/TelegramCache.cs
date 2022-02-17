using TL;

namespace TgNews.BL.Client;

public class TelegramCache
{
    internal TelegramCache()
    {

    }

    public IReadOnlyDictionary<long, ChatBase> ChatsBase => _chats;
    private readonly Dictionary<long, ChatBase> _chats = new();

    public IReadOnlyDictionary<string, Channel> Channels => _channels;
    private readonly Dictionary<string, Channel> _channels = new();
    
    public IReadOnlyDictionary<long, UserBase> Users => _users;
    private readonly Dictionary<long, UserBase> _users = new();

    internal void Subscription(IObject arg)
    {
        if (arg is not UpdatesBase updates)
        {
            return;
        }

        if (updates.Chats.Any())
        {
            foreach (var kv in updates.Chats)
            {
                switch (kv.Value)
                {
                    case Channel channel:
                        _channels[channel.username] = channel;
                        break;
                    default:
                        _chats[kv.Key] = kv.Value;
                        break;
                }
            }
        }
        
        if (updates.Users.Any())
        {
            foreach (var kv in updates.Users)
            {
                _users[kv.Key] = kv.Value;
            }
        }
    }
}