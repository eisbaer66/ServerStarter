using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerStarter.Shared;

namespace ServerStarter.Server.Services
{
    public interface IServerInfoService
    {
        Task<IList<ServerPlayer>> GetPlayersAsync(string ipAndPort);
    }
}
