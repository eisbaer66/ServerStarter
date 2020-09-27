using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerStarter.Server.Data.Repositories;
//using Microsoft.EntityFrameworkCore;
using ServerStarter.Server.Models;
using ServerStarter.Server.util;
using ServerStarter.Shared;
using Community = ServerStarter.Shared.Community;
using CommunityServer = ServerStarter.Shared.CommunityServer;

namespace ServerStarter.Server.Services
{
    public interface ICommunityService
    {
        Task<Community> UpdateCommunity(Models.Community community, CancellationToken cancellationToken);
    }

    public class CommunityService : ICommunityService
    {
        private readonly IServerInfoService     _serverInfoService;
        private readonly ICommunityQueueService _queue;
        private readonly ICommunityState        _state;
        private readonly IUserRepository        _users;

        public CommunityService(IServerInfoService     serverInfoService,
                                ICommunityQueueService queue,
                                IUserRepository        users,
                                ICommunityState        state)
        {
            _serverInfoService = serverInfoService ?? throw new ArgumentNullException(nameof(serverInfoService));
            _queue             = queue             ?? throw new ArgumentNullException(nameof(queue));
            _users             = users             ?? throw new ArgumentNullException(nameof(users));
            _state             = state             ?? throw new ArgumentNullException(nameof(state));
        }

        public async Task<Community> UpdateCommunity(Models.Community community, CancellationToken cancellationToken)
        {
            Community updatedCommunity = await CreateUpdatedCommunity(community, cancellationToken);

            await _state.UpdateLastCommunities(updatedCommunity);

            return updatedCommunity;
        }

        private async Task<Community> CreateUpdatedCommunity(Models.Community community, CancellationToken cancellationToken)
        {
            var servers = await community.Servers
                                         .Select(async s =>
                                                 {
                                                     var info = await _serverInfoService.GetPlayersAsync(s.Ip, cancellationToken);
                                                     return new CommunityServer
                                                            {
                                                                Name           = s.Name,
                                                                Ip             = s.Ip,
                                                                MaxPlayers     = info.MaxPlayers,
                                                                CurrentPlayers = info.Players.Count,
                                                                Players        = info.Players,
                                                                ConsideredFull = info.Players.Count >= community.MaximumPlayers
                                                     };
                                                 })
                                         .WhenAllList();

            var queuedPlayers  = await _queue.GetQueuedPlayers(community.Id);
            var waitingPlayers = await GetWaitingPlayers(queuedPlayers, servers);
            var queuedCommunityPlayers = queuedPlayers.Select(p => new CommunityPlayer
                                                                   {
                                                                       Name = p.Name,
                                                                   })
                                                      .ToList();

            var queueServer = servers.Where(s => !s.ConsideredFull)
                                     .Aggregate<CommunityServer, CommunityServer>(null, (a, s) =>
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

            return new Community
                   {
                       Id             = community.Id,
                       Name           = community.Name,
                       MinimumPlayers = community.MinimumPlayers,
                       CurrentPlayers = currentPlayers,
                       WaitingPlayers = waitingPlayers.Count,
                       Servers        = servers.ToList(),
                       QueuedPlayers  = queuedCommunityPlayers,
                   };
        }

        private async Task<IList<ApplicationUser>> GetWaitingPlayers(IList<ApplicationUser> queuedUsers, IList<CommunityServer> servers)
        {
            var playingSteamIds = servers
                                  .SelectMany(s => s.Players)
                                  .Select(p => p.SteamId);
            var playingUsers = await _users.GetForSteamIds(playingSteamIds);
            return _queue.GetWaitingPlayers(queuedUsers, playingUsers);
        }
    }
}