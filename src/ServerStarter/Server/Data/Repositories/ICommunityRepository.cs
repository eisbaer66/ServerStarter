using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServerStarter.Shared;

namespace ServerStarter.Server.Data.Repositories
{
    public interface ICommunityRepository
    {
        Task<IEnumerable<Models.Community>> Get();
        Task<Models.Community>              Get(Guid     id);
        Task<Models.File>                   GetIcon(Guid id, CancellationToken ct);
    }
}