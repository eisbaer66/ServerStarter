using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ServerStarter.Server.Services
{
    class InMemoryCommunityQueue : ICommunityQueue
    {
        readonly         IDictionary<Guid, HashSet<Guid>> _queues = new Dictionary<Guid, HashSet<Guid>>();
        private readonly ILogger<InMemoryCommunityQueue>  _logger;

        public InMemoryCommunityQueue(ILogger<InMemoryCommunityQueue> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Join(Guid communityId, Guid userId)
        {
            if (!_queues.ContainsKey(communityId))
                _queues.Add(communityId, new HashSet<Guid>());

            var queue = _queues[communityId];
            if (queue.Contains(userId))
                return;

            queue.Add(userId);
            _logger.LogInformation("enqueued {UserId} for {CommunityId}", userId, communityId);
        }

        public void Leave(Guid communityId, Guid userId)
        {
            if (!_queues.ContainsKey(communityId))
                return;

            var queue = _queues[communityId];
            if (!queue.Contains(userId))
                return;

            queue.Remove(userId);
            _logger.LogInformation("dequeued {UserId} for {CommunityId}", userId, communityId);
        }

        public IEnumerable<Guid> GetWaitingPlayers(Guid communityId, IList<Guid> playingUserIds)
        {
            if (!_queues.ContainsKey(communityId))
                yield break;

            var playingLookup = playingUserIds.ToHashSet();
            var queue         = _queues[communityId];
            foreach (Guid queuedId in queue)
            {
                if (playingLookup.Contains(queuedId))
                    continue;
                yield return queuedId;
            }
        }

        public IEnumerable<Guid> GetWaitingCommunityIds()
        {
            return _queues.Where(p => p.Value.Count > 0)
                          .Select(p => p.Key);
        }
    }
}