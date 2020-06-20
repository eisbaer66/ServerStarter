using System.Threading;
using System.Threading.Tasks;
using Zarlo.Stats.Data;

namespace Zarlo.Stats
{
    public interface IServerInfoQueries
    {
        Task<Server[]> GetServers(CancellationToken cancellationToken);
        Task<OnlinePlayerInfo[]> GetOnlinePlayers(CancellationToken cancellationToken);
    }
}