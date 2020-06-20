using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Zarlo.Stats;
using Zarlo.Stats.Data;

namespace ServerStarter.Server.ZarloAdapter
{
    public interface IServerInfoCache
    {
        void Reset();
    }

    public class CachingServerInfoQueries : IServerInfoQueries, IServerInfoCache
    {
        public static string GetServersKey { get { return "_GetServers"; } }
        public static string GetOnlinePlayersKey { get { return "_GetOnlinePlayers"; } }

        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheLength;
        private readonly IServerInfoQueries _next;
        private readonly ILogger<CachingServerInfoQueries> _logger;
        private readonly SemaphoreSlim _getServerSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _getOnlinePlayersSemaphore = new SemaphoreSlim(1, 1);

        public CachingServerInfoQueries(IMemoryCache cache, ITimingSettings settings, IServerInfoQueries next, ILogger<CachingServerInfoQueries> logger)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheLength = settings.CacheLength;
        }

        public void Reset()
        {
            _cache.Remove(GetServersKey);
            _cache.Remove(GetOnlinePlayersKey);
        }

        public async Task<Zarlo.Stats.Data.Server[]> GetServers(CancellationToken cancellationToken)
        {
            var servers = await GetCache(GetServersKey, _getServerSemaphore, async () => await _next.GetServers(cancellationToken), cancellationToken);

            return servers;
        }

        public async Task<OnlinePlayerInfo[]> GetOnlinePlayers(CancellationToken cancellationToken)
        {
            return await GetCache(GetOnlinePlayersKey, _getOnlinePlayersSemaphore, async () => await _next.GetOnlinePlayers(cancellationToken), cancellationToken);
        }

        public async Task<T> GetCache<T>(string key, SemaphoreSlim semaphore, Func<Task<T>> create, CancellationToken cancellationToken)
        {
            // Look for cache key.
            if (_cache.TryGetValue(key, out T cacheEntry))
            {
                _logger.LogTrace("hit cache for {CacheKey}", key);
                return cacheEntry;
            }

            // Wait for semaphore and check cache again
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                if (_cache.TryGetValue(key, out cacheEntry))
                {
                    _logger.LogTrace("hit cache for {CacheKey} after waiting", key);
                    return cacheEntry;
                }

                _logger.LogTrace("missed cache for {CacheKey}", key);
                // Key not in cache, so get data.
                cacheEntry = await create();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetAbsoluteExpiration(_cacheLength);

                // Save data in cache.
                _cache.Set(key, cacheEntry, cacheEntryOptions);
            }
            finally
            {
                semaphore.Release();
            }

            return cacheEntry;
        }
    }
}
