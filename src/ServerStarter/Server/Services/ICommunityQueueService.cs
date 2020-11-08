using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Services
{
    public interface ICommunityQueueService
    {
        Task                         Join(Guid                 communityId, string userId);
        Task                         Leave(Guid                communityId, string userId);
        Task                         LeaveAllQueues(string     userId);
        Task<IList<ApplicationUser>> GetQueuedPlayers(Guid     communityId);
        Task<CommunityQueue[]>       GetQueuedCommunity(string userId);
        Task<bool>                   Contains(string           userId, Guid communityId);
    }
}