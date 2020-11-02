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

        private IServiceProvider                _serviceProvider;
        private ILogger<CommunityUpdateService> _logger;
        private IServerInfoCache                _cache;
        private ICommunityQueueService          _communityQueueService;
        private ICommunityService               _communityService;

        [SetUp]
        public void Setup()
        {
            _cache                 = Substitute.For<IServerInfoCache>();
            _communityQueueService = Substitute.For<ICommunityQueueService>();
            _communityService      = Substitute.For<ICommunityService>();

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
            _cache.When(c => c.Reset())
                  .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityQueueService, _communityService, _timingSettings);

            await update.UpdateCommunities(CancellationToken.None);

            _cache.Received().Reset();
        }

        [Test]
        public async Task ExceptionInCommunityQueueServiceGetsSwallowed()
        {
            _communityQueueService.When(c => c.GetWaitingCommunityIds())
                                  .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityQueueService, _communityService, _timingSettings);

            await update.UpdateCommunities(CancellationToken.None);

            _cache.Received().Reset();
            await _communityQueueService.Received().GetWaitingCommunityIds();
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
            _communityQueueService.GetWaitingCommunityIds().Returns(communities);
            _communityService.When(c => c.UpdateCommunity(community, cancellationToken))
                             .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityQueueService, _communityService, _timingSettings);

            await update.UpdateCommunities(cancellationToken);

            _cache.Received().Reset();
            await _communityQueueService.Received().GetWaitingCommunityIds();
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
            _communityQueueService.GetWaitingCommunityIds().Returns(communities);
            _communityService.When(c => c.UpdateCommunity(community, cancellationToken))
                             .Throw(new Exception("Test"));
            _communityService.UpdateCommunity(community2, cancellationToken).Returns(async (ci) =>
                                                                                    {
                                                                                        await Task.Delay(20, cancellationToken);
                                                                                        return updatedCommunity;
                                                                                    });

            var update = new CommunityUpdateService(_logger, _cache, _communityQueueService, _communityService, _timingSettings);

            await update.UpdateCommunities(cancellationToken);

            _cache.Received().Reset();
            await _communityQueueService.Received().GetWaitingCommunityIds();
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
            _communityQueueService.GetWaitingCommunityIds().Returns(communities);
            _communityService.UpdateCommunity(community, cancellationToken).Returns(async (ci) =>
                                                                                    {
                                                                                        await Task.Delay(20, cancellationToken);
                                                                                        return updatedCommunity;
                                                                                    });

            var update = new CommunityUpdateService(_logger, _cache, _communityQueueService, _communityService, _timingSettings);

            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);

            _cache.Received(1).Reset();
            await _communityQueueService.Received(1).GetWaitingCommunityIds();
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
            _communityQueueService.GetWaitingCommunityIds().Returns(communities);
            _communityService.UpdateCommunity(community, cancellationToken).Returns(async (ci) =>
                                                                                    {
                                                                                        await Task.Delay(20, cancellationToken);
                                                                                        return updatedCommunity;
                                                                                    });

            var update = new CommunityUpdateService(_logger, _cache, _communityQueueService, _communityService, _timingSettings);

            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);
            update.UpdateCommunities(cancellationToken);

            await Task.Delay(80, cancellationToken);

            _cache.Received(2).Reset();
            await _communityQueueService.Received(2).GetWaitingCommunityIds();
            await _communityService.Received(2).UpdateCommunity(community, cancellationToken);
        }
    }
}