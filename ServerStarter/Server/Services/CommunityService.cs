using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.Models;
using ServerStarter.Server.Services;
using ServerStarter.Server.util;
using Community = ServerStarter.Shared.Community;
using CommunityServer = ServerStarter.Shared.CommunityServer;

namespace ServerStarter.Server.Controllers
{
    public interface ICommunityService
    {
        Task<Community> UpdateCommunity(Models.Community community);
    }

    public class CommunityService : ICommunityService
    {
        private readonly IServerInfoService           _serverInfoService;
        private readonly ICommunityQueue              _queue;
        private readonly ICommunityState             _state;
        private readonly DbSet<ApplicationUser>       _users;

        public CommunityService(IServerInfoService     serverInfoService,
                                ICommunityQueue        queue,
                                DbSet<ApplicationUser> users,
                                ICommunityState       state)
        {
            _serverInfoService = serverInfoService ?? throw new ArgumentNullException(nameof(serverInfoService));
            _queue             = queue             ?? throw new ArgumentNullException(nameof(queue));
            _users             = users             ?? throw new ArgumentNullException(nameof(users));
            _state             = state             ?? throw new ArgumentNullException(nameof(state));
        }

        public async Task<Community> UpdateCommunity(Models.Community community)
        {
            Community updatedCommunity = await CreateUpdatedCommunity(community);

            await _state.UpdateLastCommunities(updatedCommunity);

            return updatedCommunity;
        }

        private async Task<Community> CreateUpdatedCommunity(Models.Community community)
        {
            var servers = await community.Servers
                                         .Select(async s =>
                                                 {
                                                     var players = await _serverInfoService.GetPlayersAsync(s.Ip);
                                                     return new CommunityServer
                                                            {
                                                                Name           = s.Name,
                                                                Ip             = s.Ip,
                                                                CurrentPlayers = players.Count,
                                                                Players        = players,
                                                            };
                                                 })
                                         .WhenAllList();

            var waitingPlayers = await GetWaitingPlayers(community, servers);

            return new Community
                   {
                       Id             = community.Id,
                       Name           = community.Name,
                       MinimumPlayers = community.MinimumPlayers,
                       CurrentPlayers = servers.Select(s => s.CurrentPlayers).Max(),
                       WaitingPlayers = waitingPlayers.Count,
                       Servers        = servers.ToList(),
                   };
        }

        private async Task<List<Guid>> GetWaitingPlayers(Models.Community community, IList<CommunityServer> servers)
        {
            var playingSteamIds = servers
                                  .SelectMany(s => s.Players)
                                  .Select(p => p.SteamId);
            var playingUsers = await _users
                                     .Where(u => playingSteamIds.Contains(u.SteamId))
                                     .ToListAsync();
            var playingUserIds = playingUsers
                                 .Select(t => new Guid(t.Id))
                                 .ToList();
            var waitingPlayers = _queue.GetWaitingPlayers(community.Id, playingUserIds)
                                       .ToList();
            return waitingPlayers;
        }
    }
}