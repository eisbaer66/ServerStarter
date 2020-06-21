using System;
using System.Collections.Generic;

namespace ServerStarter.Server.Services
{
    public interface ICommunityQueue
    {
        void Join(Guid communityId, Guid userId);
        void Leave(Guid communityId, Guid userId);
        IEnumerable<Guid> GetWaitingPlayers(Guid communityId, IList<Guid> playingUserIds);
        IEnumerable<Guid> GetWaitingCommunityIds();
        void LeaveAllQueues(Guid userId);
        event EventHandler<UserJoinedEventArgs> UserJoined;
        event EventHandler<UserLeftEventArgs> UserLeft;
        IEnumerable<Guid> GetQueuedCommunity(Guid userId);
    }
}