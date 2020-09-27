using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;
using ServerStarter.Shared;
using Zarlo.Stats;

namespace ServerStarter.Server.ZarloAdapter
{
    class ServerInfoService : IServerInfoService
    {
        private readonly ILogger<ServerInfoService> _logger;
        private readonly IServerInfoQueries         _queries;

        public ServerInfoService(ILogger<ServerInfoService> logger, IServerInfoQueries queries)
        {
            _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<ServerInfo> GetPlayersAsync(string ipAndPort, CancellationToken cancellationToken)
        {
            var server = (await _queries.GetServers(cancellationToken))
                .FirstOrDefault(s => s.Address == ipAndPort);
            if (server == null)
            {
                _logger.LogError("no server found for {IpAndPort}", ipAndPort);
                return new ServerInfo();
            }

            int serverId = server.Id;

            var players = (await _queries.GetOnlinePlayers(cancellationToken))
                                .Where(p => p.ServerId == serverId)
                                .Where(p => p.SteamId > 0)
                                .Select(p => new ServerPlayer
                                             {
                                                 SteamId = p.SteamId,
                                             })
                                .ToList();
            return new ServerInfo
                   {
                       MaxPlayers = server.MaxPlayers,
                       Players    = players
                   };
        }
    }

    public class ServerInfo
    {
        public int MaxPlayers { get; set; }
        public IList<ServerPlayer> Players { get; set; } = new List<ServerPlayer>();
    }
}