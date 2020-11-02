using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.WorkerServices
{
    public class CommunityQueueUpdateWorker : TimedHostedService
    {
        private readonly IServiceProvider        _serviceProvider;
        private readonly IBackgroundTaskQueue    _taskQueue;

        public CommunityQueueUpdateWorker(ILogger<CommunityQueueUpdateWorker> logger,
                                          IServiceProvider                    serviceProvider,
                                          IBackgroundTaskQueue                taskQueue,
                                          ITimingSettings                     settings) : base(logger, settings.Interval)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _taskQueue       = taskQueue       ?? throw new ArgumentNullException(nameof(taskQueue));
        }

        protected override void DoWork(object state)
        {
            _taskQueue.QueueBackgroundWorkItem(async c =>
                                               {
                                                   using IServiceScope outerScope    = _serviceProvider.CreateScope();
                                                   var                 updateService = outerScope.ServiceProvider.GetRequiredService<ICommunityUpdateService>();
                                                   await updateService.UpdateCommunities(c);
                                               });
        }
    }
}