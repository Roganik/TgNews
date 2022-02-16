using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TgNews.BL.Client;
using TgNews.BL.Services;
using TL;

namespace TgNews.BL.TgEventHandlers;

public class ForwardInterestingPostsFromEventsCommand
{
    private readonly TelegramBot _bot;
    private readonly TgSubscriptionsProvider _subscriptionsProvider;
    private readonly ILogger _logger;
    private readonly SubscriptionService _subscriptionService;
    
    private readonly ConcurrentQueue<(long PeerId, Message Msg)> _unprocessedMessages = new();
    private readonly string _forwardToChannel;

    public ForwardInterestingPostsFromEventsCommand(
        Client.TelegramBot bot,
        TgNewsConfiguration cfg,
        SubscriptionService subscriptionService,
        TgSubscriptionsProvider subscriptionsProvider,
        ILoggerFactory loggerFactory)
    {
        _bot = bot;
        _subscriptionsProvider = subscriptionsProvider;
        _subscriptionService = subscriptionService;
        _forwardToChannel = cfg.TgBotForwardToChannel;
        _logger = loggerFactory.CreateLogger("ForwardNewPosts");
    }

    public void Subscribe(Telegram tg)
    {
        tg.Events.Channel.OnEditMessage += (update, msg) =>
        {
            var peerId = msg.peer_id.ID;
            _unprocessedMessages.Enqueue((peerId, msg));
        };

        tg.Events.Channel.OnNewMessage += (update, msg) =>
        {
            var peerId = msg.peer_id.ID;
            _unprocessedMessages.Enqueue((peerId, msg));
        };
    }

    public async Task Execute()
    {
        var newMessages = GetAllUnprocessedMessages()
            .ToList();
        if (newMessages.Count == 0)
        {
            _logger.LogInformation($"Don't have new messages to analyze");
            return;
        }

        var peers = newMessages
            .Select(m => m.PeerId)
            .Distinct()
            .Select(peerId => new
            {
                Subscription = _subscriptionsProvider.GetByPeerId(peerId),
                PeerId = peerId,
            })
            .ToList();
        
        _logger.LogInformation($"Got {newMessages.Count,2} new messages from {peers.Count,2} peers to analyze");
        
        foreach (var peer in peers)
        {
            var subscription = await peer.Subscription;
            if (subscription == null)
            {
                // don't have subscription for this peer
                continue;
            }

            var lastProcessedMsg = _subscriptionService.GetLastForwardedMsgId(subscription);
            var messagesToAnalyze = newMessages.Where(p => p.PeerId == peer.PeerId && p.Msg.id > lastProcessedMsg).ToList();
            if (messagesToAnalyze.Count == 0)
            {
                continue;
            }

            var interestingMessages = messagesToAnalyze.Where(m => subscription.IsMessageInteresting(m.Msg)).ToList();
            if (interestingMessages.Count == 0)
            {
                continue;
            }
            
            var msgIdToForward = interestingMessages.Select(m => m.Msg.id).ToArray();
            await _bot.ForwardMessages(msgIdToForward, subscription.ChannelName, _forwardToChannel);
            _subscriptionService.SaveLastForwardedMsgId(subscription, msgIdToForward.Max());

            _logger.LogInformation($"Forwarded {interestingMessages.Count,2} interesting messages from {subscription.ChannelName}");
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