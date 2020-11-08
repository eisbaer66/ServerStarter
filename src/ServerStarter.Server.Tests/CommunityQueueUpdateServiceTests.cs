using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Serilog;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.Models;
using ServerStarter.Server.Services;
using ServerStarter.Server.ZarloAdapter;

namespace ServerStarter.Server.Tests
{
    public class CommunityQueueUpdateServiceTests
    {
        private readonly ITimingSettings _timingSettings = new TimingSettings
                                                           {
                                                               UpdateCommunityMaxDuration   = TimeSpan.FromSeconds(10),
                                                           };

        private ILogger<CommunityUpdateService>      _logger;
        private IServerInfoCache                     _cache;
        private ICommunityRepository                 _communityRepository;
        private ICommunityService                    _communityService;
        private ICommunityServiceCache               _serviceCache;

        [SetUp]
        public void Setup()
        {
            _cache               = Substitute.For<IServerInfoCache>();
            _communityRepository = Substitute.For<ICommunityRepository>();
            _communityService    = Substitute.For<ICommunityService>();
            _serviceCache        = Substitute.For<ICommunityServiceCache>();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.NUnitOutput()
                .CreateLogger();
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog());

            var serviceProvider = services.BuildServiceProvider();
            _logger          = serviceProvider.GetRequiredService<ILogger<CommunityUpdateService>>();
        }

        [Test]
        public async Task ExceptionInCacheGetsSwallowed()
        {
            _serviceCache.Set(null)
                         .ThrowsForAnyArgs(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _serviceCache);

            await update.UpdateCommunities(CancellationToken.None);

            _cache.Received().Reset();
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
            _communityRepository.Get().Returns(communities);
            _communityService.When(c => c.UpdateCommunity(community, cancellationToken))
                             .Throw(new Exception("Test"));

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _serviceCache);

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
            _communityRepository.Get().Returns(communities);
            _communityService.When(c => c.UpdateCommunity(community, cancellationToken))
                             .Throw(new Exception("Test"));
            _communityService.UpdateCommunity(community2, cancellationToken).Returns(async (ci) =>
                                                                                    {
                                                                                        await Task.Delay(20, cancellationToken);
                                                                                        return updatedCommunity;
                                                                                    });

            var update = new CommunityUpdateService(_logger, _cache, _communityRepository, _communityService, _timingSettings, _serviceCache);

            await update.UpdateCommunities(cancellationToken);

            _cache.Received().Reset();
            await _communityRepository.Received().Get();
            await _communityService.Received().UpdateCommunity(community, cancellationToken);
            await _communityService.Received().UpdateCommunity(community2, cancellationToken);
        }
    }
}