using System.Threading.Tasks;
using Zarlo.Stats.Data;

namespace Zarlo.Stats
{
    public interface IServerInfoQueries
    {
        Task<Server[]>           GetServers();
        Task<OnlinePlayerInfo[]> GetOnlinePlayers();
    }
}