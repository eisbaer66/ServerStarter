using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.Models;
using ServerStarter.Server.Services;
using ServerStarter.Server.WorkerServices;
using ServerStarter.Server.ZarloAdapter;

namespace ServerStarter.Server.Tests
{
    public class CommunityQueueUpdateServiceTests
    {
        private readonly ITimingSettings _timingSettings = new TimingSettings
                                                           {
                                                               UpdateCommunityMaxDuration   = TimeSpan.FromSeconds(10),
                                                               UpdateCommunitiesMaxDuration = TimeSpan.FromSeconds(30)
                                                           };

        private IServiceProvider                     _serviceProvider;
        private ILogger<CommunityUpdateService>      _logger;
        private IServerInfoCache                     _cache;
        private ICommunityRepository                 _communityRepository;
        private ICommunityService                    _communityService;
        private IHubConnectionSource<CommunitiesHub> _hubConnections;

        [SetUp]
        public void Setup()
        {
            _cache                 = Substitute.For<IServerInfoCache>();
            _communityRepository = Substitute.For<ICommunityRepository>();
            _communityService      = Substitute.For<ICommunityService>();
            _hubConnections        = Substitute.For<IHubConnectionSource<CommunitiesHub>>();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.NUnitOutput()
                .CreateLogger();
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog());

            _serviceProvider = services.BuildServiceProvider();
            _logger          = _serviceProvider.GetRequiredService<ILogger<CommunityUpdateService>>();
        }

        [Test]
        public async Task ExceptionInCacheGetsSwallowed()
        {
            _hubConnections.UsersConnected().Returns(true);
            _cache.When(c => c.Reset())
                  .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            await update.UpdateCommunities(CancellationToken.None);

            _cache.Received().Reset();
        }

        [Test]
        public async Task ExceptionInHubConnectionGetsSwallowed()
        {
            _hubConnections.When(c => c.UsersConnected())
                           .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            await update.UpdateCommunities(CancellationToken.None);

            _hubConnections.Received().UsersConnected();
        }

        [Test]
        public async Task ExceptionInCommunityQueueServiceGetsSwallowed()
        {
            _hubConnections.UsersConnected().Returns(true);
            _communityRepository.When(c => c.Get())
                                  .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            await update.UpdateCommunities(CancellationToken.None);

            _cache.Received().Reset();
            await _communityRepository.Received().Get();
        }

        [Test]
        public async Task ExceptionInCommunityServiceGetsSwallowed()
        {
            var cancellationToken = CancellationToken.None;

            var community = new Community
                            {
                                Id = Guid.NewGuid(),
                            };
            var communities = new[]
                              {
                                  community
                              };
            _hubConnections.UsersConnected().Returns(true);
            _communityRepository.Get().Returns(communities);
            _communityService.When(c => c.UpdateCommunity(community, cancellationToken))
                             .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            await update.UpdateCommunities(cancellationToken);

            _cache.Received().Reset();
            await _communityRepository.Received().Get();
            await _communityService.Received().UpdateCommunity(community, cancellationToken);
        }

        [Test]
        public async Task ExceptionWhileUpdatingOneCommunityDoesNotBlockUpdatesOnTheNextCommunity()
        {
            var cancellationToken = CancellationToken.None;

            var community = new Community
                            {
                                Id = Guid.NewGuid(),
                            };
            var updatedCommunity = new Shared.Community
                                   {
                                       Id = Guid.NewGuid(),
                                   };
            var community2 = new Community
                            {
                                Id = Guid.NewGuid(),
                            };
            var communities = new[]
                              {
                                  community,
                                  community2,
                              };
            _hubConnections.UsersConnected().Returns(true);
            _communityRepository.Get().Returns(communities);
            _communityService.When(c => c.UpdateCommunity(community, cancellationToken))
                             .Throw(new Exception("Test"));
            _communityService.UpdateCommunity(community2, cancellationToken).Returns(async (ci) =>
                                                                                    {
                                                                                        await Task.Delay(20, cancellationToken);
                                                                                        return updatedCommunity;
                                                                                    });

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            await update.UpdateCommunities(cancellationToken);

            _cache.Received().Reset();
            await _communityRepository.Received().Get();
            await _communityService.Received().UpdateCommunity(community, cancellationToken);
            await _communityService.Received().UpdateCommunity(community2, cancellationToken);
        }

        [Test]
        public async Task BulkheadAllowsDirectExecution()
        {
            var cancellationToken = CancellationToken.None;

            var community = new Community
                            {
                                Id = Guid.NewGuid(),
                            };
            var updatedCommunity = new Shared.Community
                                   {
                                       Id = Guid.NewGuid(),
                                   };
            var communities = new[]
                              {
                                  community
                              };
            _hubConnections.UsersConnected().Returns(true);
            _communityRepository.Get().Returns(communities);
            _communityService.UpdateCommunity(community, cancellationToken).Returns(async (ci) =>
                                                                                    {
                                                                                        await Task.Delay(20, cancellationToken);
                                                                                        return updatedCommunity;
                                                                                    });

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);

            _cache.Received(1).Reset();
            await _communityRepository.Received(1).Get();
            await _communityService.Received(1).UpdateCommunity(community, cancellationToken);
        }

        [Test]
        public async Task BulkheadAllowsOnlyOneFollowupExecution()
        {
            var cancellationToken = CancellationToken.None;

            var community = new Community
                            {
                                Id = Guid.NewGuid(),
                            };
            var updatedCommunity = new Shared.Community
            {
                                Id = Guid.NewGuid(),
                            };
            var communities = new[]
                              {
                                  community
                              };
            _hubConnections.UsersConnected().Returns(true);
            _communityRepository.Get().Returns(communities);
            _communityService.UpdateCommunity(community, cancellationToken).Returns(async (ci) =>
                                                                                    {
                                                                                        await Task.Delay(20, cancellationToken);
                                                                                        return updatedCommunity;
                                                                                    });

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);

            await Task.Delay(80, cancellationToken);

            _cache.Received(2).Reset();
            await _communityRepository.Received(2).Get();
            await _communityService.Received(2).UpdateCommunity(community, cancellationToken);
        }

        [Test]
        public async Task SkipsExecutionIfNoHubConnectionIsFound()
        {
            var cancellationToken = CancellationToken.None;

            var community = new Community
                            {
                                Id = Guid.NewGuid(),
                            };
            _hubConnections.UsersConnected().Returns(false);

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _hubConnections);

            await update.UpdateCommunities(cancellationToken);

            _cache.Received(0).Reset();
            await _communityRepository.Received(0).Get();
            await _communityService.Received(0).UpdateCommunity(community, cancellationToken);
        }
    }
}