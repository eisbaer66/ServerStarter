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
        private readonly ICommunityQueue      _queue;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServerInfoCache     _cache;
        private          bool                 _running = false;

        public CommunityQueueUpdate(IServiceProvider serviceProvider,
                                    ILogger<CommunityQueueUpdate> logger,
                                    ICommunityQueue queue,
                                    IBackgroundTaskQueue          taskQueue,
                                    ITimingSettings settings,
                                    IServerInfoCache cache) : base(logger, settings.Interval)
        {
            _queue           = queue           ?? throw new ArgumentNullException(nameof(queue));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _taskQueue       = taskQueue       ?? throw new ArgumentNullException(nameof(taskQueue));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected override void DoWork(object state)
        {
            if (_running)
                return;
            
            _running = true;
            _cache.Reset();
            IEnumerable<Guid> waitingCommunityIds = _queue.GetWaitingCommunityIds();
            foreach (Guid communityId in waitingCommunityIds)
            {
                _taskQueue.QueueBackgroundWorkItem(async c =>
                                                   {
                                                       using (IServiceScope scope = _serviceProvider.CreateScope())
                                                       {
                                                           var repository = scope.ServiceProvider.GetRequiredService<ICommunityRepository>();
                                                           var service = scope.ServiceProvider.GetRequiredService<ICommunityService>();

                                                           var communityData = await repository.Get(communityId);
                                                           await service.UpdateCommunity(communityData, StoppingToken);
                                                       }
                                                   });
            }
            _taskQueue.QueueBackgroundWorkItem(async c => { _running = false; });       //TODO Ghetto-Wait
        }
    }
}