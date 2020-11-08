using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Data.Repositories.Queues
{
    class InMemoryCommunityQueueRepository : ICommunityQueueRepository
    {
        readonly IDictionary<Guid, CommunityQueue> _queues = new Dictionary<Guid, CommunityQueue>();

        public async Task<CommunityQueue> Get(Guid communityId)
        {
            if (!_queues.ContainsKey(communityId))
                return null;

            return _queues[communityId];
        }

        public async Task<Community[]> GetWaitingCommunityIds()
        {
            return _queues.Where(p => p.Value.Entries.Count > 0)
                          .Select(p => p.Value.Community)
                          .ToArray();
        }

        public async Task<CommunityQueue[]> GetQueuedFor(string userId)
        {
            return _queues.Where(p => p.Value.Contains(userId))
                          .Select(p => p.Value)
                          .ToArray();
        }

        public async Task<bool> IsQueuedFor(string userId, Guid communityId)
        {
            if (!_queues.ContainsKey(communityId))
                return false;

            return _queues[communityId].Contains(userId);
        }

        public void Add(CommunityQueue queue)
        {
            if (!_queues.ContainsKey(queue.Community.Id))
                _queues.Add(queue.Community.Id, queue);
        }
    }
}