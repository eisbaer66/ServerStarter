using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ServerStarter.Server.Hubs
{
    [Authorize]
    public class CommunitiesHub : Hub
    {
        public async Task<string> GetName() //TODO figure out how to set NameClaimType correctly
        {
            return Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }
        public async Task SendMessage(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("MessageReceived", groupName, await GetName(), message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserJoined", groupName, await GetName());
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserLeft", groupName, await GetName());
        }
    }
}
