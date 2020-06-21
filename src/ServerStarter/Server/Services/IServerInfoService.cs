using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServerStarter.Server.ZarloAdapter;
using ServerStarter.Shared;

namespace ServerStarter.Server.Services
{
    public interface IServerInfoService
    {
        Task<ServerInfo> GetPlayersAsync(string ipAndPort, CancellationToken cancellationToken);
    }
}
