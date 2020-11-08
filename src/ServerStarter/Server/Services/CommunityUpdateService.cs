using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.ZarloAdapter;

namespace ServerStarter.Server.Services
{
    public interface ICommunityUpdateService
    {
        Task UpdateCommunities(CancellationToken cancellationToken);
    }

    public class CommunityUpdateService : ICommunityUpdateService
    {
        private readonly IAsyncPolicy                    _communityPolicy;
        private readonly IServerInfoCache                _serverInfoCache;
        private readonly ICommunityServiceCache          _cache;
        private readonly ICommunityRepository            _repository;
        private readonly ICommunityService               _service;
        private readonly ILogger<CommunityUpdateService> _logger;

        public CommunityUpdateService(ILogger<CommunityUpdateService> logger,
                                      IServerInfoCache                serverInfoCache,
                                      ICommunityRepository            repository,
                                      ICommunityService               service,
                                      ITimingSettings                 timingSettings,
                                      ICommunityServiceCache          cache)
        {
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
            _serverInfoCache = serverInfoCache ?? throw new ArgumentNullException(nameof(serverInfoCache));
            _repository      = repository      ?? throw new ArgumentNullException(nameof(repository));
            _service         = service         ?? throw new ArgumentNullException(nameof(service));
            _cache           = cache           ?? throw new ArgumentNullException(nameof(cache));

            _communityPolicy = Policy.WrapAsync(Policy.Handle<Exception>()
                                                      .FallbackAsync(async ct => { },
                                                                     async e => _logger.LogError(e, "Error occurred updating Community")),
                                                Policy.TimeoutAsync(timingSettings.UpdateCommunityMaxDuration));
        }

        public async Task UpdateCommunities(CancellationToken cancellationToken)
        {
            await UpdateCommunitiesInternal(cancellationToken);
        }

        private async Task UpdateCommunitiesInternal(CancellationToken cancellationToken)
        {
            _serverInfoCache.Reset();
            var communities = await _repository.Get();
            foreach (var community in communities)
            {
                using (_logger.BeginScope("Community {@CommunityId}", community.Id))
                {
                    await _communityPolicy.ExecuteAsync(async () =>
                                                        {
                                                            _logger.LogTrace("inner CommunityQueueUpdate-workitem started");

                                                            var updatedCommunity = await _service.UpdateCommunity(community, cancellationToken);
                                                            _cache.Set(updatedCommunity);

                                                            _logger.LogTrace("inner CommunityQueueUpdate-workitem finished");
                                                        });
                }
            }
        }
    }
}