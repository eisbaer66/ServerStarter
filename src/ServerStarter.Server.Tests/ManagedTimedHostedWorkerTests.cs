using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using Serilog;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.Services;
using ServerStarter.Server.WorkerServices;

namespace ServerStarter.Server.Tests
{
    [TestFixture]
    public class ManagedTimedHostedWorkerTests
    {
        private readonly IManagedTimedHostedWorkerSettings _settings = new ManagedTimedHostedWorkerSettings
                                                                       {
                                                                           MaxDuration = TimeSpan.FromSeconds(30)
                                                                       };

        private ServiceProvider                      _serviceProvider;
        private ILogger<ManagedTimedHostedWorker>    _logger;
        private IHubConnectionSource<CommunitiesHub> _connectionSource;

        [SetUp]
        public void Setup()
        {
            Log.Logger = new LoggerConfiguration()
                         .WriteTo.NUnitOutput()
                         .CreateLogger();
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog());

            _serviceProvider = services.BuildServiceProvider();
            _logger          = _serviceProvider.GetRequiredService<ILogger<ManagedTimedHostedWorker>>();

            _connectionSource = Substitute.For<IHubConnectionSource<CommunitiesHub>>();
        }

        [Test]
        public async Task Test()
        {
            var called = false;
            Func<IServiceProvider, CancellationToken, Task> work = async (sp, ct) =>
                                                                   {
                                                                       called = true;
                                                                       throw new Exception("Test");
                                                                   };

            var queue      = new TaskQueue();
            var worker     = new ManagedTimedHostedWorkerUnderTest(async (sp, ct) => { called = true; }, queue, _logger, _settings, _serviceProvider, _connectionSource);

            await worker.StartAsync(CancellationToken.None);
            await queue.Work(CancellationToken.None);
            await worker.StopAsync(CancellationToken.None);

            Assert.IsTrue(called, "WorkItem not called");
        }

        [Test]
        public async Task ExceptionInCacheGetsSwallowed()
        {
            var called = false;
            Func<IServiceProvider, CancellationToken, Task> work = async (sp, ct) =>
                                                                   {
                                                                       called = true;
                                                                       throw new Exception("Test");
                                                                   };

            var queue  = new TaskQueue();
            var worker = new ManagedTimedHostedWorkerUnderTest(work, queue, _logger, _settings, _serviceProvider, _connectionSource);

            await worker.StartAsync(CancellationToken.None);
            await queue.Work(CancellationToken.None);
            await worker.StopAsync(CancellationToken.None);

            Assert.IsTrue(called, "WorkItem not called");
        }

        [Test]
        public async Task BulkheadAllowsOnlyOneFollowupExecution()
        {
            var called     = 0;
            Func<IServiceProvider, CancellationToken, Task> work = async (sp, ct) =>
                                                                   {
                                                                       called++;
                                                                       await Task.Delay(10, ct);
                                                                   };

            var queue = new TaskQueue();
            var settings = new ManagedTimedHostedWorkerSettings
                           {
                               Interval    = TimeSpan.FromMilliseconds(5),
                               MaxDuration = TimeSpan.FromSeconds(30),
                           };
            var worker = new ManagedTimedHostedWorkerUnderTest(work, queue, _logger, settings, _serviceProvider, _connectionSource);

            await worker.StartAsync(CancellationToken.None);
            await Task.Delay(20);
            await queue.Work(CancellationToken.None, false);
            await worker.StopAsync(CancellationToken.None);

            Assert.AreEqual(2, called, "WorkItem not called");
        }

        [Test]
        public async Task SkipsExecutionIfNoHubConnectionIsFound()
        {
            var called = 0;
            Func<IServiceProvider, CancellationToken, Task> work = async (sp, ct) =>
                                                                   {
                                                                       called++;
                                                                   };

            var queue = new TaskQueue();
            var settings = new ManagedTimedHostedWorkerSettings
                           {
                               Interval                      = TimeSpan.FromMilliseconds(5),
                               MaxDuration                   = TimeSpan.FromSeconds(30),
                               OnlyRunIfHubConnectionPresent = true,
                           };
            _connectionSource.UsersConnected().Returns(false);
            var worker = new ManagedTimedHostedWorkerUnderTest(work, queue, _logger, settings, _serviceProvider, _connectionSource);

            await worker.StartAsync(CancellationToken.None);
            await Task.Delay(20);
            await queue.Work(CancellationToken.None, false);
            await worker.StopAsync(CancellationToken.None);

            Assert.AreEqual(0, called, "WorkItem called");
        }
    }

    public class ManagedTimedHostedWorkerUnderTest : ManagedTimedHostedWorker
    {
        private readonly Func<IServiceProvider, CancellationToken, Task> _work;

        public ManagedTimedHostedWorkerUnderTest(Func<IServiceProvider, CancellationToken, Task> work,
                                                 IBackgroundTaskQueue                            taskQueue,
                                                 ILogger<ManagedTimedHostedWorker>               logger,
                                                 IManagedTimedHostedWorkerSettings               settings,
                                                 IServiceProvider                                serviceProvider,
                                                 IHubConnectionSource<CommunitiesHub>            connectionSource) 
            : base(taskQueue, logger, settings, serviceProvider, connectionSource)
        {
            _work = work ?? throw new ArgumentNullException(nameof(work));
        }

        protected override string Name            => nameof(ManagedTimedHostedWorkerUnderTest);
        protected override string TransactionName => "testing";
        protected override string TransactionType => "test";
        protected override async Task   Execute(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            await _work(serviceProvider, cancellationToken);
        }
    }

    public class TaskQueue : IBackgroundTaskQueue
    {
        private readonly ManualResetEvent                     _resetEvent = new ManualResetEvent(false);
        private readonly IList<Func<CancellationToken, Task>> _workItems  = new List<Func<CancellationToken, Task>>();

        public void                                QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            _workItems.Add(workItem);

            _resetEvent.Set();
        }

        public async Task Work(CancellationToken cancellationToken, bool atLeastOneWorkItem = true)
        {
            if (atLeastOneWorkItem && _workItems.Count == 0)
                _resetEvent.WaitOne();

            var tasks = _workItems.Select(w => w(cancellationToken)).ToArray();
            Task.WaitAll(tasks);
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            return async (ct) => { };
        }
    }
}