using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ServerStarter.Shared;

namespace ServerStarter.Server.Services
{
    public interface ICommunityServiceCache
    {
        Community Set(Community community);
    }

    public class InMemoryCommunityServiceCache : ICommunityService, ICommunityServiceCache
    {
        public const     string            CacheKey = "InMemoryCommunityServiceCache_";
        private readonly ICommunityService _service;
        private readonly IMemoryCache      _cache;
        private readonly ITimingSettings   _timingSettings;

        public InMemoryCommunityServiceCache(ICommunityService service, IMemoryCache cache, ITimingSettings timingSettings)
        {
            _service        = service        ?? throw new ArgumentNullException(nameof(service));
            _cache          = cache          ?? throw new ArgumentNullException(nameof(cache));
            _timingSettings = timingSettings ?? throw new ArgumentNullException(nameof(timingSettings));
        }

        public async Task<Community> UpdateCommunity(Models.Community community, CancellationToken cancellationToken)
        {
            return await _cache.GetOrCreateAsync(CacheKey + community.Id,
                                                 async e =>
                                                 {
                                                     e.AbsoluteExpirationRelativeToNow = _timingSettings.CommunityUpdateCacheDuration;
                                                     return await _service.UpdateCommunity(community, cancellationToken);
                                                 });
        }
        
        public Community Set(Community community)
        {
            return _cache.Set(CacheKey + community.Id, community, _timingSettings.CommunityUpdateCacheDuration);
        }
    }
}