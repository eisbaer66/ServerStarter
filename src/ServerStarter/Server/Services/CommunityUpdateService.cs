using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using ServerStarter.Server.ZarloAdapter;

namespace ServerStarter.Server.Services
{
    public interface ICommunityUpdateService
    {
        Task UpdateCommunities(CancellationToken cancellationToken);
    }

    public class CommunityUpdateService : ICommunityUpdateService
    {
        private readonly IAsyncPolicy                    _policy;
        private readonly IAsyncPolicy                    _communityPolicy;
        private readonly IServerInfoCache                _cache;
        private readonly ICommunityQueueService          _queue;
        private readonly ICommunityService               _service;
        private readonly ILogger<CommunityUpdateService> _logger;

        public CommunityUpdateService(ILogger<CommunityUpdateService> logger,
                                      IServerInfoCache                cache,
                                      ICommunityQueueService          queue,
                                      ICommunityService               service,
                                      ITimingSettings                 timingSettings)
        {
            if (timingSettings == null) throw new ArgumentNullException(nameof(timingSettings));
            _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
            _cache   = cache   ?? throw new ArgumentNullException(nameof(cache));
            _queue   = queue   ?? throw new ArgumentNullException(nameof(queue));
            _service = service ?? throw new ArgumentNullException(nameof(service));

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
                                           _cache.Reset();

                                           _logger.LogDebug("outer CommunityQueueUpdate-workitem started");
                                           await SetupCommunityUpdates(cancellationToken);
                                           _logger.LogDebug("outer CommunityQueueUpdate-workitem finished");

                                           _logger.LogInformation("finished CommunityQueueUpdate");
                                       });
        }

        private async Task SetupCommunityUpdates(CancellationToken cancellationToken)
        {
            var waitingCommunities = await _queue.GetWaitingCommunityIds();
            foreach (var community in waitingCommunities)
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