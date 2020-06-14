using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;
using SteamQueryNet;
using SteamQueryNet.Interfaces;
using SteamQueryNet.Models;

namespace ServerStarter.Server.SteamQueryNetAdapters
{
    class ServerInfoService : IServerInfoService
    {
        private readonly ILogger<ServerInfoService> _logger;

        public ServerInfoService(ILogger<ServerInfoService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public byte GetPlayerCount(string ipAndPort)
        {
            IServerQuery serverQuery = new ServerQuery(ipAndPort);
            ServerInfo   info        = serverQuery.GetServerInfo();
            _logger.LogInformation("received {@ServerInfo}", info);
            return info.Players;
        }

        public async Task<byte> GetPlayerCountAsync(string ipAndPort)
        {
            IServerQuery serverQuery = new ServerQuery(ipAndPort);
            ServerInfo   info        = await serverQuery.GetServerInfoAsync();
            _logger.LogInformation("received {@ServerInfo}", info);
            return info.Players;
        }
    }
}