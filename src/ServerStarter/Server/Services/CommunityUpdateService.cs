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
        private readonly IAsyncPolicy                         _policy;
        private readonly IAsyncPolicy                         _communityPolicy;
        private readonly IServerInfoCache                     _cache;
        private readonly ICommunityRepository                 _repository;
        private readonly ICommunityService                    _service;
        private readonly ILogger<CommunityUpdateService>      _logger;
        private readonly IHubConnectionSource<CommunitiesHub> _connectionSource;

        public CommunityUpdateService(ILogger<CommunityUpdateService>      logger,
                                      IServerInfoCache                     cache,
                                      ICommunityRepository                 repository,
                                      ICommunityService                    service,
                                      ITimingSettings                      timingSettings,
                                      IHubConnectionSource<CommunitiesHub> connectionSource)
        {
            if (timingSettings == null) throw new ArgumentNullException(nameof(timingSettings));
            _logger           = logger           ?? throw new ArgumentNullException(nameof(logger));
            _cache            = cache            ?? throw new ArgumentNullException(nameof(cache));
            _repository       = repository       ?? throw new ArgumentNullException(nameof(repository));
            _service          = service          ?? throw new ArgumentNullException(nameof(service));
            _connectionSource = connectionSource ?? throw new ArgumentNullException(nameof(connectionSource));

            _policy = Policy.WrapAsync(Policy.Handle<Exception>()
                                             .FallbackAsync(async ct => { },
                                                            async e => _logger.LogError(e, "Error occurred updating CommunityQueues")),
                                       Policy.BulkheadAsync(1, 1, async ct => _logger.LogWarning("skipped running CommunityQueueUpdate, because it is already running")),
                                       Policy.TimeoutAsync(timingSettings.UpdateCommunitiesMaxDuration));
            _communityPolicy = Policy.WrapAsync(Policy.Handle<Exception>()
                                                      .FallbackAsync(async ct => { },
                                                                     async e => _logger.LogError(e, "Error occurred updating Community")),
                                                Policy.TimeoutAsync(timingSettings.UpdateCommunityMaxDuration));
        }

        public async Task UpdateCommunities(CancellationToken cancellationToken)
        {
            await _policy.ExecuteAsync(async () =>
                                       {
                                           _logger.LogInformation("running CommunityQueueUpdate");

                                           await UpdateCommunitiesIfNeeded(cancellationToken);

                                           _logger.LogInformation("finished CommunityQueueUpdate");
                                       });
        }

        private async Task UpdateCommunitiesIfNeeded(CancellationToken cancellationToken)
        {
            if (!_connectionSource.UsersConnected())
            {
                _logger.LogInformation("no user is connected. skipping update of communities");
                return;
            }
            
            _logger.LogDebug("outer CommunityQueueUpdate-workitem started");
            await UpdateCommunitiesInternal(cancellationToken);
            _logger.LogDebug("outer CommunityQueueUpdate-workitem finished");
        }

        private async Task UpdateCommunitiesInternal(CancellationToken cancellationToken)
        {
            _cache.Reset();
            var communities = await _repository.Get();
            foreach (var community in communities)
            {
                using (_logger.BeginScope("Community {@CommunityId}", community.Id))
                {
                    await _communityPolicy.ExecuteAsync(async () =>
                                                        {
                                                            _logger.LogTrace("inner CommunityQueueUpdate-workitem started");

                                                            await _service.UpdateCommunity(community, cancellationToken);

                                                            _logger.LogTrace("inner CommunityQueueUpdate-workitem finished");
                                                        });
                }
            }
        }
    }
}