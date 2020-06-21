using System.Threading.Tasks;
using ServerStarter.Shared;

namespace ServerStarter.Server.Services
{
    public interface ICommunityState
    {
        Task UpdateLastCommunities(Community community);
    }
}