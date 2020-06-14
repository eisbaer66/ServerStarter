using ServerStarter.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Data;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Services;
using ServerStarter.Server.SteamQueryNetAdapters;
using ServerStarter.Server.util;
using SteamQueryNet;
using SteamQueryNet.Interfaces;
using SteamQueryNet.Models;

namespace ServerStarter.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CommunitiesController : ControllerBase
    {
        private readonly IServerInfoService _serverInfoService;
        private readonly ICommunityRepository _repository;

        public CommunitiesController(ICommunityRepository repository, IServerInfoService serverInfoService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _serverInfoService = serverInfoService ?? throw new ArgumentNullException(nameof(serverInfoService));
        }

        [HttpGet]
        public async Task<IEnumerable<Community>> Get()
        {
            var communities = await _repository.Get();

            return await communities
                         .Select(GetCommunityData)
                         .WhenAll();
        }

        private async Task<Community> GetCommunityData(Models.Community community)
        {
            var servers = await community.Servers
                                         .Select(async s =>
                                                 {
                                                     var playerCount = await _serverInfoService.GetPlayerCountAsync(s.Ip);
                                                     return new CommunityServer
                                                            {
                                                                Name           = s.Name,
                                                                Ip             = s.Ip,
                                                                CurrentPlayers = playerCount,
                                                            };
                                                 })
                                         .WhenAllList();

            return new Community
                   {
                       Id             = community.Id,
                       Name           = community.Name,
                       MinimumPlayers = community.MinimumPlayers,
                       CurrentPlayers = servers.Select(s => s.CurrentPlayers).Max(),
                       WaitingPlayers = 0, //TODO Waiting players
                       Servers        = servers.ToList(),
                   };
        }
    }
}
