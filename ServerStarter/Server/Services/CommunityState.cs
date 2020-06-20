using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ServerStarter.Server.Hubs;
using ServerStarter.Shared;

namespace ServerStarter.Server.Services
{
    class CommunityState : ICommunityState
    {
        private readonly IDictionary<Guid, Community> _lastCommunities = new Dictionary<Guid, Community>();
        private readonly IHubContext<CommunitiesHub>  _hub;

        public CommunityState(IHubContext<CommunitiesHub> hub)
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        public async Task UpdateLastCommunities(Community updatedCommunity)
        {
            if (!_lastCommunities.ContainsKey(updatedCommunity.Id))
            {
                _lastCommunities.Add(updatedCommunity.Id, updatedCommunity);
                return;
            }

            Community lastCommunity = _lastCommunities[updatedCommunity.Id];

            if (!lastCommunity.Equals(updatedCommunity))
                await _hub.Clients.NotifyCommunityChange(lastCommunity.Id);

            _lastCommunities[updatedCommunity.Id] = updatedCommunity;
        }
    }
}