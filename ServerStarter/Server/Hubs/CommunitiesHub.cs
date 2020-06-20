using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.Hubs
{
    [Authorize]
    public class CommunitiesHub : Hub
    {
        private readonly ICommunityQueue _queue;
        private readonly ILogger<CommunitiesHub> _logger;
        private readonly IDictionary<Guid, HashSet<string>> _connections = new Dictionary<Guid, HashSet<string>>();

        public CommunitiesHub(ICommunityQueue queue, ILogger<CommunitiesHub> logger)
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

        public async Task SendMessage(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("MessageReceived", groupName, Context.User.GetName(), message);
        }

        public async Task JoinGroup(string groupName)
        {
            var userId = Context.User.GetUserId();
            var communityId = new Guid(groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
             _queue.Join(communityId, userId);
        }

        public async Task LeaveGroup(string groupName)
        {
            var userId      = Context.User.GetUserId();
            var communityId = new Guid(groupName);

            _queue.Leave(communityId, userId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            AddConnection();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
                _logger.LogError(exception, "SignalR fatal disconnect");

            RemoveConnection();

            return base.OnDisconnectedAsync(exception);
        }

        private void AddConnection()
        {
            var userId = Context.User.GetUserId();
            if (!_connections.ContainsKey(userId))
                _connections.Add(userId, new HashSet<string>());
            _connections[userId].Add(Context.ConnectionId);
        }

        private void RemoveConnection()
        {
            var userId = Context.User.GetUserId();
            if (_connections.ContainsKey(userId))
            {
                if (_connections[userId].Contains(Context.ConnectionId))
                {
                    _connections[userId].Remove(Context.ConnectionId);
                    if (_connections[userId].Count == 0)
                    {
                        _connections.Remove(userId);
                        _queue.LeaveAllQueues(userId);
                    }
                }
            }
        }
    }
}
