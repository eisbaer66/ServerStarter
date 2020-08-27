using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ServerStarter.Server.Services
{
    public class UserJoinedEventArgs
    {
        public Guid CommunityId { get; set; }
        public Guid UserId { get; set; }
    }
    public class UserLeftEventArgs
    {
        public Guid CommunityId { get; set; }
        public Guid UserId { get; set; }
    }

    class InMemoryCommunityQueue : ICommunityQueue
    {
        readonly         IDictionary<Guid, HashSet<Guid>> _queues = new Dictionary<Guid, HashSet<Guid>>();
        private readonly ILogger<InMemoryCommunityQueue>  _logger;

        public event EventHandler<UserJoinedEventArgs>    UserJoined;
        public event EventHandler<UserLeftEventArgs>      UserLeft;

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
            UserJoined?.Invoke(this, new UserJoinedEventArgs{CommunityId = communityId, UserId = userId});
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
            UserLeft?.Invoke(this, new UserLeftEventArgs { CommunityId = communityId, UserId = userId });
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

        public void LeaveAllQueues(Guid userId)
        {
            foreach (var p in _queues)
            {
                if (!p.Value.Contains(userId))
                    continue;
                p.Value.Remove(userId);
                UserLeft?.Invoke(this, new UserLeftEventArgs { CommunityId = p.Key, UserId = userId });
            }
        }

        public IEnumerable<Guid> GetQueuedCommunity(Guid userId)
        {
            //TODO keep reverse index if we get to many communities
            foreach (var p in _queues)
            {
                if (p.Value.Contains(userId))
                    yield return p.Key;
            }
        }
        public bool Contains(Guid userId, Guid communityId)
        {
            if (!_queues.ContainsKey(communityId))
                return false;

            return _queues[communityId].Contains(userId);
        }
    }
}