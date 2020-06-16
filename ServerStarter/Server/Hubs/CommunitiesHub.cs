using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.Hubs
{
    [Authorize]
    public class CommunitiesHub : Hub
    {
        private readonly ICommunityQueue _queue;

        public CommunitiesHub(ICommunityQueue queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public async Task SendMessage(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("MessageReceived", groupName, Context.User.GetName(), message);
        }

        public async Task JoinGroup(string groupName)
        {
            var userId = Context.User.GetUserId();
            var communityId = new Guid(groupName);
            _queue.Join(communityId, userId);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserJoined", groupName, Context.User.GetName());
            await Clients.All.SendAsync("Changed", groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            var userId      = Context.User.GetUserId();
            var communityId = new Guid(groupName);
            _queue.Leave(communityId, userId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserLeft", groupName, Context.User.GetName());
            await Clients.All.SendAsync("Changed", groupName);
        }
    }
}
