using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;
using ServerStarter.Server.ZarloAdapter;

namespace ServerStarter.Server.WorkerServices
{
    public class CommunityQueueUpdate : TimedHostedService
    {
        private readonly IServiceProvider              _serviceProvider;
        private readonly IBackgroundTaskQueue          _taskQueue;
        private readonly IServerInfoCache              _cache;
        private readonly ILogger<CommunityQueueUpdate> _logger;
        private          bool                          _running = false;

        public CommunityQueueUpdate(IServiceProvider              serviceProvider,
                                    ILogger<CommunityQueueUpdate> logger,
                                    IBackgroundTaskQueue          taskQueue,
                                    ITimingSettings               settings,
                                    IServerInfoCache              cache) : base(logger, settings.Interval)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
            _taskQueue       = taskQueue       ?? throw new ArgumentNullException(nameof(taskQueue));
            _cache           = cache           ?? throw new ArgumentNullException(nameof(cache));
        }

        protected override void DoWork(object state)
        {
            if (_running)
            {
                _logger.LogWarning("skipped running CommunityQueueUpdate, because it is already running");
                return;
            }
            
            _logger.LogInformation("running CommunityQueueUpdate");
            _running = true;
            _cache.Reset();

            _taskQueue.QueueBackgroundWorkItem(async c =>
            {
                _logger.LogDebug("outer CommunityQueueUpdate-workitem started");
                using IServiceScope    outerScope         = _serviceProvider.CreateScope();
                ICommunityQueueService queue              = outerScope.ServiceProvider.GetRequiredService<ICommunityQueueService>();
                var                    waitingCommunities = await queue.GetWaitingCommunityIds();
                foreach (var community in waitingCommunities)
                {
                    _taskQueue.QueueBackgroundWorkItem(async c =>
                                                       {
                                                           _logger.LogTrace("inner CommunityQueueUpdate-workitem started for {@Community}", community);
                                                           using IServiceScope scope   = _serviceProvider.CreateScope();
                                                           var                 service = scope.ServiceProvider.GetRequiredService<ICommunityService>();

                                                           await service.UpdateCommunity(community, StoppingToken);
                                                           _logger.LogTrace("inner CommunityQueueUpdate-workitem finished for {@Community}", community);
                                                       });
                }

                _logger.LogDebug("outer CommunityQueueUpdate-workitem finished");
                _logger.LogInformation("finished CommunityQueueUpdate");
                _running = false;
            });
        }
    }
}