using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TgNews.BL.Client;
using TgNews.BL.Repositories;
using TL;

namespace TgNews.BL.TgEventHandlers;

public class ChannelEventHandler : IDisposable
{
    private readonly ChannelMsgsRepository _repo;
    private readonly ConcurrentQueue<(UpdateNewChannelMessage, Message)> _unprocessedMessages = new();
    private Timer _timer = null!;
    private readonly TgSubscriptionsProvider _subscriptionsProvider;

    public ChannelEventHandler(ChannelMsgsRepository repo,
        TgSubscriptionsProvider subscriptionsProvider,
        ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _subscriptionsProvider = subscriptionsProvider;
    }

    public void Subscribe(Telegram tg)
    {
        tg.Events.Channel.OnNewMessage += (channelMsg, message) =>
        {
            _unprocessedMessages.Enqueue((channelMsg, message));
        };

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(4));
    }

    private async void DoWork(object? state)
    {
        if (!_unprocessedMessages.TryDequeue(out var item))
        {
            return;
        }

        var newMsg = item.Item1;
        var msg = item.Item2;

        var peer = await _subscriptionsProvider.GetByPeerId(msg.peer_id.ID);
        var channel = peer?.ChannelName;
        if (channel == null)
        {
            return;
        }

        var json = SerializeToJson(newMsg);
        _repo.Insert(channel, msg.message, msg.Date, json);
    }

    private string SerializeToJson(UpdateNewChannelMessage msg)
    {
        var updateType = msg.GetType();
        var serializerOpts = new JsonSerializerSettings() { Formatting = Formatting.Indented };
        var updateJson = JsonConvert.SerializeObject(msg, updateType, serializerOpts);
        return updateJson;
    }


    public void Dispose()
    {
        _timer.Dispose();
    }
}