using System;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.WorkerServices
{
    public class CommunityQueueUpdateWorker : ManagedTimedHostedWorker
    {
        public CommunityQueueUpdateWorker(ILogger<CommunityQueueUpdateWorker>  logger,
                                          IServiceProvider                     serviceProvider,
                                          IBackgroundTaskQueue                 taskQueue,
                                          IManagedTimedHostedWorkerSettings    managedSettings,
                                          IHubConnectionSource<CommunitiesHub> connectionSource)
            : base(taskQueue, logger, managedSettings, serviceProvider, connectionSource)
        {
        }

        protected override string Name            => nameof(CommunityQueueUpdateWorker);
        protected override string TransactionName => "update CommunityQueue";
        protected override string TransactionType => ApiConstants.TypeExternal;
        protected override async Task Execute(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var updateService = serviceProvider.GetRequiredService<ICommunityUpdateService>();
            await updateService.UpdateCommunities(cancellationToken);
        }
    }
}