using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerStarter.Server.Data.Repositories
{
    public interface ICommunityRepository
    {
        Task<IEnumerable<Models.Community>> Get();
    }
}