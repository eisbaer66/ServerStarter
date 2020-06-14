using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerStarter.Server.Services
{
    public interface IServerInfoService
    {
        byte GetPlayerCount(string ipAndPort);
        Task<byte> GetPlayerCountAsync(string ipAndPort);
    }
}
