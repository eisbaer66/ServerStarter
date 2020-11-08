using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Hubs;

namespace ServerStarter.Server.WorkerServices
{
    public class CommunityCacheUpdateWorker : ManagedTimedHostedWorker
    {

        public CommunityCacheUpdateWorker(IBackgroundTaskQueue                 taskQueue,
                                          ILogger<CommunityCacheUpdateWorker>  logger,
                                          IServiceProvider                     serviceProvider,
                                          IManagedTimedHostedWorkerSettings    managedSettings,
                                          IHubConnectionSource<CommunitiesHub> connectionSource) 
            : base(taskQueue, logger, managedSettings, serviceProvider, connectionSource)
        {
        }

        protected override string Name            => nameof(CommunityCacheUpdateWorker);
        protected override string TransactionName => "warming up community-cache";
        protected override string TransactionType => ApiConstants.TypeDb;

        protected override async Task Execute(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var repository  = serviceProvider.GetRequiredService<CommunityRepository>();
            var cache      = serviceProvider.GetRequiredService<ICommunityRepositoryCache>();

            var communities = (await repository.Get()).ToArray();

            cache.Set(communities);
            foreach (var community in communities)
            {
                cache.Set(community);
            }
        }
    }
}
