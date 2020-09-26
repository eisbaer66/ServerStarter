using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Controllers;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Services;
using ServerStarter.Server.ZarloAdapter;

namespace ServerStarter.Server.WorkerServices
{
    public class CommunityQueueUpdate : TimedHostedService
    {
        private readonly IServiceProvider     _serviceProvider;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServerInfoCache     _cache;
        private          bool                 _running = false;

        public CommunityQueueUpdate(IServiceProvider              serviceProvider,
                                    ILogger<CommunityQueueUpdate> logger,
                                    IBackgroundTaskQueue          taskQueue,
                                    ITimingSettings               settings,
                                    IServerInfoCache              cache) : base(logger, settings.Interval)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _taskQueue       = taskQueue       ?? throw new ArgumentNullException(nameof(taskQueue));
            _cache           = cache           ?? throw new ArgumentNullException(nameof(cache));
        }

        protected override void DoWork(object state)
        {
            if (_running)
                return;
            
            _running = true;
            _cache.Reset();

            _taskQueue.QueueBackgroundWorkItem(async c =>
            {
                using IServiceScope    outerScope         = _serviceProvider.CreateScope();
                ICommunityQueueService queue              = outerScope.ServiceProvider.GetRequiredService<ICommunityQueueService>();
                var                    waitingCommunities = await queue.GetWaitingCommunityIds();
                foreach (var community in waitingCommunities)
                {
                    _taskQueue.QueueBackgroundWorkItem(async c =>
                                                       {
                                                           using IServiceScope scope      = _serviceProvider.CreateScope();
                                                           var                 service    = scope.ServiceProvider.GetRequiredService<ICommunityService>();

                                                           await service.UpdateCommunity(community, StoppingToken);
                                                       });
                }

                _running = false;
            });
        }
    }
}