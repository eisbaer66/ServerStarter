using System;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Apm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using ServerStarter.Server.Hubs;

namespace ServerStarter.Server.WorkerServices
{
    public abstract class ManagedTimedHostedWorker : TimedHostedService
    {
        private readonly IBackgroundTaskQueue                 _taskQueue;
        private readonly ILogger<ManagedTimedHostedWorker>    _logger;
        private readonly IManagedTimedHostedWorkerSettings    _settings;
        private readonly IServiceProvider                     _serviceProvider;
        private readonly IAsyncPolicy                         _policy;
        private readonly IHubConnectionSource<CommunitiesHub> _connectionSource;

        protected ManagedTimedHostedWorker(IBackgroundTaskQueue                 taskQueue,
                                           ILogger<ManagedTimedHostedWorker>    logger,
                                           IManagedTimedHostedWorkerSettings    settings,
                                           IServiceProvider                     serviceProvider,
                                           IHubConnectionSource<CommunitiesHub> connectionSource) :
            base(logger, settings.Interval)
        {
            _taskQueue        = taskQueue        ?? throw new ArgumentNullException(nameof(taskQueue));
            _logger           = logger           ?? throw new ArgumentNullException(nameof(logger));
            _settings         = settings         ?? throw new ArgumentNullException(nameof(settings));
            _serviceProvider  = serviceProvider  ?? throw new ArgumentNullException(nameof(serviceProvider));
            _connectionSource = connectionSource ?? throw new ArgumentNullException(nameof(connectionSource));

            _policy = Policy.WrapAsync(Policy.Handle<Exception>()
                                             .FallbackAsync(async ct => { },
                                                            async e => _logger.LogError(e, "Error occurred running ManagedTimedHostedWorker {WorkerName}", Name)),
                                       Policy.BulkheadAsync(1, 1, async ct => _logger.LogWarning("skipped running ManagedTimedHostedWorker {WorkerName}, because it is already running", Name)),
                                       Policy.TimeoutAsync(settings.MaxDuration));
        }

        protected override void DoWork(object state)
        {
            if (ShouldSkip())
                return;

            _taskQueue.QueueBackgroundWorkItem(async c =>
                                               {
                                                   //TODO initialize Agent before first request?
                                                   //might even skip execution if noone has accessed the site jet?
                                                   if (!Agent.IsConfigured)
                                                   {
                                                       await SetupAndExecute(c);
                                                       return;
                                                   }
                                                   await Agent.Tracer.CaptureTransaction(TransactionName,
                                                                                         TransactionType, 
                                                                                         async () => await SetupAndExecute(c));
                                               });
        }

        private async Task SetupAndExecute(CancellationToken cancellationToken)
        {
            await _policy.ExecuteAsync(async (ct) =>
                                       {
                                           using IServiceScope outerScope = _serviceProvider.CreateScope();
                                           await Execute(outerScope.ServiceProvider, ct);

                                           _logger.LogInformation("finished ManagedTimedHostedWorker {WorkerName}", Name);
                                       },
                                       cancellationToken);
        }

        protected virtual bool ShouldSkip()
        {
            if (!_settings.OnlyRunIfHubConnectionPresent)
                return false;

            if (!_connectionSource.UsersConnected())
            {
                _logger.LogInformation("no user is connected. skipping update of communities");
                return true;
            }

            return false;
        }

        protected abstract string Name { get; }
        protected abstract string TransactionName { get; }
        protected abstract string TransactionType { get; }
        protected abstract Task   Execute(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}