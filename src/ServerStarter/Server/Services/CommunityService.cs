using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Models;
using ServerStarter.Server.util;

namespace ServerStarter.Server.Services
{
    public interface ICommunityService
    {
        Task<CommunityUpdate> UpdateCommunity(Community community, CancellationToken cancellationToken);
    }

    public class CommunityService : ICommunityService
    {
        private readonly IServerInfoService        _serverInfoService;
        private readonly ICommunityQueueService    _queue;
        private readonly ICommunityState           _state;
        private readonly ILogger<CommunityService> _logger;

        public CommunityService(IServerInfoService        serverInfoService,
                                ICommunityQueueService    queue,
                                ICommunityState           state,
                                ILogger<CommunityService> logger)
        {
            _serverInfoService = serverInfoService ?? throw new ArgumentNullException(nameof(serverInfoService));
            _queue             = queue             ?? throw new ArgumentNullException(nameof(queue));
            _state             = state             ?? throw new ArgumentNullException(nameof(state));
            _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<CommunityUpdate> UpdateCommunity(Community community, CancellationToken cancellationToken)
        {
            CommunityUpdate updatedCommunity = await CreateUpdatedCommunity(community, cancellationToken);
            _logger.LogDebug("updated community {@UpdatedCommunity}", updatedCommunity);

            await _state.UpdateLastCommunities(updatedCommunity);

            return updatedCommunity;
        }

        private async Task<CommunityUpdate> CreateUpdatedCommunity(Community community, CancellationToken cancellationToken)
        {
            var servers = await community.Servers
                                         .Select(async s =>
                                                 {
                                                     var info = await _serverInfoService.GetPlayersAsync(s.Ip, cancellationToken);
                                                     return new CommunityUpdateServer
                                                     {
                                                                Name           = s.Name,
                                                                Ip             = s.Ip,
                                                                MaxPlayers     = info.MaxPlayers,
                                                                CurrentPlayers = info.Players.Count,
                                                                Players        = CommunityUpdatePlayer.From(info.Players),
                                                                ConsideredFull = info.Players.Count >= community.MaximumPlayers
                                                     };
                                                 })
                                         .WhenAllList();

            var queuedPlayers  = await _queue.GetQueuedPlayers(community.Id);
            var waitingPlayers = FilterWaitingPlayers(queuedPlayers, servers);
            var queuedCommunityPlayers = queuedPlayers.Select(p => new CommunityUpdatePlayer
                                                                   {
                                                                       SteamId = p.SteamId,
                                                                       Name = p.Name,
                                                                   })
                                                      .ToList();

            var queueServer = servers.Where(s => !s.ConsideredFull)
                                     .Aggregate<CommunityUpdateServer, CommunityUpdateServer>(null, (a, s) =>
                                                                                                    {
                                                                                                        if (a == null)
                                                                                                            return s;
                                                                                                        return s.CurrentPlayers > a.CurrentPlayers ? s : a;
                                                                                                    });

            int currentPlayers = 0;
            if (queueServer != null)
            {
                queueServer.PreferredForQueue = true;
                currentPlayers                = queueServer.CurrentPlayers;
            }

            return new CommunityUpdate
                   {
                       Id             = community.Id,
                       Name           = community.Name,
                       MinimumPlayers = community.MinimumPlayers,
                       CurrentPlayers = currentPlayers,
                       WaitingPlayers = waitingPlayers.Count,
                       Servers        = servers.ToList(),
                       QueuedPlayers  = queuedCommunityPlayers,
                       Updated        = DateTime.UtcNow,
                   };
        }

        private IList<ApplicationUser> FilterWaitingPlayers(IList<ApplicationUser> queuedUsers, IList<CommunityUpdateServer> servers)
        {
            var playingSteamIds = servers
                                  .SelectMany(s => s.Players)
                                  .Select(p => p.SteamId)
                                  .ToHashSet();
            var waitingPlayers = queuedUsers.Where(user => !playingSteamIds.Contains(user.SteamId)).ToList();
            return waitingPlayers;
        }
    }
}