using TL;

namespace TgNews.BL.Client;

public class TelegramCache
{
    internal TelegramCache()
    {

    }

    public IReadOnlyDictionary<long, ChatBase> Chats => _chats;
    private Dictionary<long, ChatBase> _chats = new();
    
    public IReadOnlyDictionary<long, UserBase> Users => _users;
    private Dictionary<long, UserBase> _users = new();

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
                _chats[kv.Key] = kv.Value;
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