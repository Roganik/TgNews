using TgNews.BL.Services;
using TgNews.BL.Subscriptions;

namespace TgNews.BL;

public class TgSubscriptionsProvider
{
    private readonly SubscriptionsConfiguration _configuration;
    private readonly SubscriptionService _subscriptionService;
    private readonly Dictionary<long, ITgSubscription> _peerIdToSubscriptionCache = new();
    private bool _isPeerIdToSubscriptionCacheInitialized = false;

    private readonly SemaphoreSlim _cacheInitSemaphoreSlim = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    public TgSubscriptionsProvider(
        SubscriptionsConfiguration configuration,
        SubscriptionService subscriptionService
        )
    {
        _configuration = configuration;
        _subscriptionService = subscriptionService;
    }

    public List<ITgSubscription> GetAll()
    {
        var result = new List<ITgSubscription>();
        foreach (var subscriptionCfg in _configuration.Subscriptions)
        {
            result.Add(new GenericSubscription(subscriptionCfg));
        }

        return result;
    }

    public async Task<ITgSubscription?> GetByPeerId(long peerId)
    {
        await InitSubscriptionsCacheIfNeeded();

        if (_peerIdToSubscriptionCache.TryGetValue(peerId, out var subscription))
        {
            return subscription;
        }

        return null;
    }

    private async Task InitSubscriptionsCacheIfNeeded()
    {
        if (!_isPeerIdToSubscriptionCacheInitialized)
        {
            await _cacheInitSemaphoreSlim.WaitAsync();
            if (!_isPeerIdToSubscriptionCacheInitialized)
            {
                await InitSubscriptionsCache();
                _isPeerIdToSubscriptionCacheInitialized = true;
            }

            _cacheInitSemaphoreSlim.Release();
        }
    }

    private async Task InitSubscriptionsCache()
    {
        var subscriptions = GetAll();
        foreach (var subscription in subscriptions)
        {
            var id = await _subscriptionService.GetTelegramSubscriptionChannelId(subscription);
            _peerIdToSubscriptionCache[id] = subscription;
        }
    }
}