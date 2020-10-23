using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Hubs;
using ServerStarter.Shared;

namespace ServerStarter.Server.Services
{
    class CommunityState : ICommunityState
    {
        private readonly IDictionary<Guid, Community> _lastCommunities = new Dictionary<Guid, Community>();
        private readonly IHubContext<CommunitiesHub>  _hub;
        private readonly ILogger<CommunityState>      _logger;

        public CommunityState(IHubContext<CommunitiesHub> hub, ILogger<CommunityState> logger)
        {
            _hub    = hub    ?? throw new ArgumentNullException(nameof(hub));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task UpdateLastCommunities(Community updatedCommunity)
        {
            if (!_lastCommunities.ContainsKey(updatedCommunity.Id))
            {
                _lastCommunities.Add(updatedCommunity.Id, updatedCommunity);

                _logger.LogInformation("new community, notifying clients {@Community}", updatedCommunity);
                await _hub.Clients.NotifyCommunityChange(updatedCommunity.Id);
                return;
            }

            Community lastCommunity = _lastCommunities[updatedCommunity.Id];

            if (!lastCommunity.Equals(updatedCommunity))
            {
                _logger.LogInformation("community changed, notifying clients {@Community}", updatedCommunity);
                await _hub.Clients.NotifyCommunityChange(lastCommunity.Id);
            }
            else
            {
                _logger.LogTrace("community did not changed, no notification send to clients {@Community}", updatedCommunity);
            }

            _lastCommunities[updatedCommunity.Id] = updatedCommunity;
        }
    }
}