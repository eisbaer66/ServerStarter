using System.Threading.Tasks;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Services
{
    public interface ICommunityState
    {
        Task UpdateLastCommunities(CommunityUpdate community);
    }
}