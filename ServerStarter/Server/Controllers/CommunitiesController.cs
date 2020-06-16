using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Models;
using ServerStarter.Server.Services;
using ServerStarter.Server.util;
using Community = ServerStarter.Shared.Community;
using CommunityServer = ServerStarter.Shared.CommunityServer;

namespace ServerStarter.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CommunitiesController : ControllerBase
    {
        private readonly IServerInfoService _serverInfoService;
        private readonly ICommunityRepository _repository;
        private readonly ICommunityQueue _queue;
        private readonly DbSet<ApplicationUser> _users;

        public CommunitiesController(ICommunityRepository   repository,
                                     IServerInfoService     serverInfoService,
                                     ICommunityQueue        queue,
                                     DbSet<ApplicationUser> users)
        {
            _repository        = repository        ?? throw new ArgumentNullException(nameof(repository));
            _serverInfoService = serverInfoService ?? throw new ArgumentNullException(nameof(serverInfoService));
            _queue             = queue             ?? throw new ArgumentNullException(nameof(queue));
            _users             = users             ?? throw new ArgumentNullException(nameof(users));
        }

        [HttpGet]
        public async Task<IEnumerable<Community>> Get()
        {
            var communities = await _repository.Get();

            return await communities
                         .Select(GetCommunityData)
                         .WhenAll();
        }

        [HttpGet("{id}")]
        public async Task<Community> Get(Guid id)
        {
            var community = await _repository.Get(id);

            return await GetCommunityData(community);
        }

        private async Task<Community> GetCommunityData(Models.Community community)
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
