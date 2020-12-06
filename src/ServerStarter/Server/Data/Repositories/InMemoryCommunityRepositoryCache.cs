using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Data.Repositories
{
    public interface ICommunityRepositoryCache
    {
        IEnumerable<Community> Set(IEnumerable<Community> communities);
        Community              Set(Community              community);
    }

    public class InMemoryCommunityRepositoryCache : ICommunityRepository, ICommunityRepositoryCache
    {
        public const     string               CacheKey = "InMemoryCommunityRepositoryCache_";
        private readonly ICommunityRepository _repository;
        private readonly IMemoryCache         _cache;
        private readonly ITimingSettings      _timingSettings;

        public InMemoryCommunityRepositoryCache(ICommunityRepository repository, IMemoryCache cache, ITimingSettings timingSettings)
        {
            _repository     = repository     ?? throw new ArgumentNullException(nameof(repository));
            _cache          = cache          ?? throw new ArgumentNullException(nameof(cache));
            _timingSettings = timingSettings ?? throw new ArgumentNullException(nameof(timingSettings));
        }

        public async Task<IEnumerable<Models.Community>> Get()
        {
            return await _cache.GetOrCreateAsync(CacheKey + "All",
                                                 async e =>
                                                 {
                                                     e.AbsoluteExpirationRelativeToNow = _timingSettings.CommunityCacheDuration;
                                                     return await _repository.Get();
                                                 });
            
        }

        public IEnumerable<Community> Set(IEnumerable<Community> communities)
        {
            return _cache.Set(CacheKey + "All", communities, _timingSettings.CommunityCacheDuration);
        }

        public async Task<Models.Community> Get(Guid id)
        {
            return await _cache.GetOrCreateAsync(CacheKey + id,
                                                 async e =>
                                                 {
                                                     e.AbsoluteExpirationRelativeToNow = _timingSettings.CommunityCacheDuration;
                                                     return await _repository.Get(id);
                                                 });
        }

        public async Task<File> GetIcon(Guid id, CancellationToken ct)
        {
            return await _cache.GetOrCreateAsync(CacheKey + "Icon" + id,
                                                 async e =>
                                                 {
                                                     e.AbsoluteExpirationRelativeToNow = _timingSettings.CommunityHeaderImageCacheDuration;
                                                     return await _repository.GetIcon(id, ct);
                                                 });
        }

        public Community Set(Community community)
        {
            return _cache.Set(CacheKey + community.Id, community, _timingSettings.CommunityCacheDuration);
        }
    }
}