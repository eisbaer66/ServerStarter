using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ServerStarter.Server.Hubs
{
    static class HubCallerClientExtensions
    {
        public static async Task NotifyCommunityChange(this IHubClients<IClientProxy> clients, Guid communityId)
        {
            await NotifyCommunityChange(clients, communityId.ToString());
        }
        public static async Task NotifyCommunityChange(this IHubClients<IClientProxy> clients, string communityId)
        {
            await clients.All.SendAsync("Changed", communityId);
        }

    }
}