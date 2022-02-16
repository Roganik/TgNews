using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TgNews.BL.Client;
using TgNews.BL.Services;
using TL;

namespace TgNews.BL.TgEventHandlers;

public class MarkSubscriptionsAsReadEventHandler
{
    private readonly Telegram _tg;
    private readonly TgSubscriptionsProvider _subscriptionsProvider;
    private readonly ILogger _logger;
    private readonly SubscriptionService _subscriptionService;
    
    private readonly ConcurrentQueue<(long PeerId, Message Msg)> _unprocessedMessages = new();


    public MarkSubscriptionsAsReadEventHandler(
        Client.Telegram tg,
        SubscriptionService service,
        TgSubscriptionsProvider subscriptionsProvider,
        ILoggerFactory loggerFactory)
    {
        _tg = tg;
        _subscriptionsProvider = subscriptionsProvider;
        _logger = loggerFactory.CreateLogger("MarkAsRead");
        _subscriptionService = service;
    }

    public void Subscribe(Telegram tg)
    {
        tg.Events.Channel.OnUpdateEditChannelMessage += (update, msg) =>
        {
            var peerId = msg.peer_id.ID;
            _unprocessedMessages.Enqueue((peerId, msg));
        };

        tg.Events.Channel.OnUpdateNewChannelMessage += (update, msg) =>
        {
            var peerId = msg.peer_id.ID;
            _unprocessedMessages.Enqueue((peerId, msg));
        };
    }

    public async Task Execute()
    {
        var newMessages = GetAllUnprocessedMessages().ToList();
        if (newMessages.Count == 0)
        {
            return;
        }

        var peerIds = newMessages.Select(m => m.PeerId).Distinct().ToList();
        foreach (var peerId in peerIds)
        {
            var subscription = await _subscriptionsProvider.GetByPeerId(peerId);
            if (subscription == null)
            {
                // don't have subscription for this peer
                continue;
            }

            if (!subscription.MarkAsReadAutomatically)
            {
                continue;
            }

            var lastProcessedMsg = _subscriptionService.GetLastReadMsgId(subscription);
            var maxMsgId = newMessages.Where(p => p.PeerId == peerId).Select(m => m.Msg.id).Max();
            if (maxMsgId == lastProcessedMsg)
            {
                continue;
            }

            _logger.LogInformation($"Marked {subscription.ChannelName, 20} as read.");
            await _tg.MarkChannelAsRead(subscription.ChannelName, maxMsgId);
            _subscriptionService.SaveLastReadMsgId(subscription, maxMsgId);
        }
    }

    private IEnumerable<(long PeerId, Message Msg)> GetAllUnprocessedMessages()
    {
        while (_unprocessedMessages.TryDequeue(out var item))
        {
            yield return item;
        }
    }
}