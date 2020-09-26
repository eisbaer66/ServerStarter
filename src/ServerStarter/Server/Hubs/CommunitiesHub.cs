using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Identity.AuthPolicies;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.Hubs
{
    [Authorize]
    public class CommunitiesHub : Hub
    {
        private readonly ICommunityQueueService               _queue;
        private readonly ILogger<CommunitiesHub>              _logger;
        private readonly IDictionary<string, HashSet<string>> _connections = new Dictionary<string, HashSet<string>>();

        public CommunitiesHub(ICommunityQueueService queue, ILogger<CommunitiesHub> logger)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queue.UserJoined += async (sender, args) =>
                                 {
                                     string groupName = args.CommunityId.ToString();
                                     string name = Context.User.GetName();
                                     await Clients.Group(groupName).SendAsync("UserJoined", groupName, name);
                                     await Clients.NotifyCommunityChange(groupName);
                                 };

            _queue.UserLeft += async (sender, args) =>
                                 {
                                     string groupName = args.CommunityId.ToString();
                                     string name = Context.User.GetName();
                                     await Clients.Group(groupName).SendAsync("UserLeft", groupName, name);
                                     await Clients.NotifyCommunityChange(groupName);
                                 };
        }

        [Authorize(Policy = Policies.JoinedQueueFromHubParameter0)]
        public async Task SendMessage(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("MessageReceived", groupName, Context.User.GetName(), message);
        }

        public async Task JoinGroup(string groupName)
        {
            var userId = Context.User.GetUserId();
            var communityId = new Guid(groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await _queue.Join(communityId, userId);

             await Clients.GroupExcept(userId.ToString(), Context.ConnectionId).SendAsync("JoinQueue", communityId);
        }

        [Authorize(Policy = Policies.JoinedQueueFromHubParameter0)]
        public async Task LeaveGroup(string groupName)
        {
            var userId      = Context.User.GetUserId();
            var communityId = new Guid(groupName);

            await _queue.Leave(communityId, userId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.GroupExcept(userId.ToString(), Context.ConnectionId).SendAsync("LeaveQueue", groupName);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            string userId = Context.User.GetUserId();
            AddConnection(userId);

            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

            await RejoinQueue(userId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
                _logger.LogError(exception, "SignalR fatal disconnect");

            string userId = Context.User.GetUserId();
            RemoveConnection(userId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());

            await base.OnDisconnectedAsync(exception);
        }

        private void AddConnection(string userId)
        {
            if (!_connections.ContainsKey(userId))
                _connections.Add(userId, new HashSet<string>());
            _connections[userId].Add(Context.ConnectionId);
        }

        private async Task RemoveConnection(string userId)
        {
            if (!_connections.ContainsKey(userId)) 
                return;
            if (!_connections[userId].Contains(Context.ConnectionId)) 
                return;

            _connections[userId].Remove(Context.ConnectionId);
            if (_connections[userId].Count != 0) 
                return;

            _connections.Remove(userId);
            await _queue.LeaveAllQueues(userId);
        }

        private async Task RejoinQueue(string userId)
        {
            var queues = await _queue.GetQueuedCommunity(userId);
            foreach (var queue in queues)
            {
                await Clients.Caller.SendAsync("JoinQueue", queue.Community.Id);
            }
        }
    }
}
