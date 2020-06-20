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

        public async Task<IList<ServerPlayer>> GetPlayersAsync(string ipAndPort, CancellationToken cancellationToken)
        {
            var server = (await _queries.GetServers(cancellationToken))
                .FirstOrDefault(s => s.PublicAddress == ipAndPort);
            if (server == null)
            {
                _logger.LogError("no server found for {IpAndPort}", ipAndPort);
                return new List<ServerPlayer>();
            }

            int serverId = server.Id;

            return (await _queries.GetOnlinePlayers(cancellationToken))
                   .Where(p => p.ServerId == serverId)
                   .Select(p => new ServerPlayer
                                {
                                    SteamId = p.SteamId,
                                })
                   .ToList();
        }
    }
}