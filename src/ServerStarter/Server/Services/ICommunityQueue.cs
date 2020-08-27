using System;
using System.Collections.Generic;

namespace ServerStarter.Server.Services
{
    public interface ICommunityQueue
    {
        void                                    Join(Guid              communityId, Guid userId);
        void                                    Leave(Guid             communityId, Guid userId);
        void                                    LeaveAllQueues(Guid    userId);
        IEnumerable<Guid>                       GetWaitingPlayers(Guid communityId, IList<Guid> playingUserIds);
        IEnumerable<Guid>                       GetWaitingCommunityIds();
        IEnumerable<Guid>                       GetQueuedCommunity(Guid userId);
        bool                                    Contains(Guid           userId, Guid communityId);
        event EventHandler<UserJoinedEventArgs> UserJoined;
        event EventHandler<UserLeftEventArgs>   UserLeft;
    }
}