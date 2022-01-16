using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TgNews.BL.Client;
using TgNews.BL.Services;
using TgNews.BL.Subscriptions;
using TL;

namespace TgNews.BL.Commands;

public class ForwardInterestingPostsFromEventsCommand
{
    private readonly Telegram _tg;
    private readonly TelegramBot _bot;
    private readonly TgNewsConfiguration _cfg;
    private readonly TgSubscriptionsProvider _subscriptionsProvider;
    private readonly ILogger _logger;
    private readonly SubscriptionService _subscriptionService;
    
    private readonly ConcurrentQueue<(long PeerId, Message Msg)> _unprocessedMessages = new();
    private readonly Dictionary<long, ITgSubscription> _peerIdToSubscriptionCache = new();

    public ForwardInterestingPostsFromEventsCommand(
        Client.Telegram tg,
        Client.TelegramBot bot,
        Client.DbStorage db,
        TgNewsConfiguration cfg,
        TgSubscriptionsProvider subscriptionsProvider,
        ILogger logger)
    {
        _tg = tg;
        _bot = bot;
        _cfg = cfg;
        _subscriptionsProvider = subscriptionsProvider;
        _logger = logger;
        _subscriptionService = new SubscriptionService(db);
    }

    public async Task Init()
    {
        _tg.Events.OnUpdateEditChannelMessage += (update, msg) =>
        {
            var peerId = msg.peer_id.ID;
            _unprocessedMessages.Enqueue((peerId, msg));
        };
        
        _logger.LogInformation("Initializing tg and bot");
        await _tg.Init();
        await _bot.Init();

        _logger.LogInformation("Initializing subscriptions cache");
        var subscriptions = _subscriptionsProvider.GetAll();
        foreach (var subscription in subscriptions)
        {
            var id = await GetChannelId(subscription);
            _peerIdToSubscriptionCache[id] = subscription;
            _logger.LogInformation($"Channel: {subscription.ChannelName,20}, ID: {id}");
        }
        
        _logger.LogInformation("Init Done");
        await Task.Delay(2000); // getting some time for a job to catch up events from tg.
    }

    private async Task<long> GetChannelId(ITgSubscription subscription)
    {
        var cachedId = _subscriptionService.GetTelegramSubscriptionChannelId(subscription);
        if (cachedId != default)
        {
            return cachedId;
        }
        
        var id = await _bot.GetChannelId(subscription.ChannelName);
        _subscriptionService.SaveTelegramSubscriptionChannelId(subscription, id);
        
        return id;
       
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
                Subscription = GetSubscription(peerId),
                PeerId = peerId,
            })
            .ToList();
        
        _logger.LogInformation($"Got {newMessages.Count,2} new messages from {peers.Count,2} peers to analyze");
        
        foreach (var peer in peers)
        {
            if (peer.Subscription == null)
            {
                // don't have subscription for this peer
                continue;
            }

            var subscription = peer.Subscription;
            var lastProcessedMsg = _subscriptionService.GetLastProcessedMsgId(subscription);
            var messagesToAnalyze = newMessages.Where(p => p.PeerId == peer.PeerId && p.Msg.id > lastProcessedMsg).ToList();
            if (messagesToAnalyze.Count == 0)
            {
                continue;
            }

            var maxMsgId = messagesToAnalyze.Select(m => m.Msg.id).Max();
            if (subscription.MarkAsReadAutomatically)
            {
                await _tg.MarkChannelAsRead(subscription.ChannelName, maxMsgId);
            }
            _subscriptionService.SaveLastProcessedMsgId(subscription, maxMsgId);

            var interestingMessages = messagesToAnalyze.Where(m => subscription.IsMessageInteresting(m.Msg)).ToList();
            if (interestingMessages.Count == 0)
            {
                continue;
            }
            
            var msgIdToForward = interestingMessages.Select(m => m.Msg.id).ToArray();
            await _bot.ForwardMessages(msgIdToForward, subscription.ChannelName, _cfg.TgBotForwardToChannel);
            
            _logger.LogInformation($"Forwarded {interestingMessages.Count,2} interesting messages from {subscription.ChannelName}");
        }
    }
    
    private ITgSubscription? GetSubscription(long peerId)
    {
        if (_peerIdToSubscriptionCache.TryGetValue(peerId, out var subscription))
        {
            return subscription;
        }

        return null;
    }
    
    private IEnumerable<(long PeerId, Message Msg)> GetAllUnprocessedMessages()
    {
        while (_unprocessedMessages.TryDequeue(out var item))
        {
            yield return item;
        }
    }
}