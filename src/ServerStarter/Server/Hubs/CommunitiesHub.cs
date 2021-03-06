﻿using System;
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
        private readonly IHubApm<CommunitiesHub>              _apm;
        private readonly IMessaging                           _messaging;
        private readonly IHubConnectionSource<CommunitiesHub> _connections;

        public CommunitiesHub(ICommunityQueueService queue, ILogger<CommunitiesHub> logger, IHubApm<CommunitiesHub> apm, IMessaging messaging, IHubConnectionSource<CommunitiesHub> connections)
        {
            _queue       = queue       ?? throw new ArgumentNullException(nameof(queue));
            _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
            _apm         = apm         ?? throw new ArgumentNullException(nameof(apm));
            _messaging   = messaging   ?? throw new ArgumentNullException(nameof(messaging));
            _connections = connections ?? throw new ArgumentNullException(nameof(connections));

            messaging.UserJoined += UserJoinedQueue;
            messaging.UserLeft   += UserLeftQueue;
        }

        protected override void Dispose(bool disposing)
        {
            _messaging.UserJoined -= UserJoinedQueue;
            _messaging.UserLeft   -= UserLeftQueue;
        }

        private async void UserJoinedQueue(object sender, UserJoinedEventArgs args)
        {
            try
            {
                string groupName = args.CommunityId.ToString();
                string name      = Context.User.GetName();
                await Clients.Group(groupName).SendAsync("UserJoined", groupName, name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error notifying existing users of newly joined user");
            }
        }

        private async void UserLeftQueue(object sender, UserLeftEventArgs args)
        {
            try
            {
                string groupName = args.CommunityId.ToString();
                string name      = Context.User.GetName();
                await Clients.Group(groupName).SendAsync("UserLeft", groupName, name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error notifying existing users of leaving user");
            }
        }

        [Authorize(Policy = Policies.JoinedQueueFromHubParameter0)]
        public async Task SendMessage(string groupName, string message)
        {
            await _apm.Trace("SendMessage", async () => {
                                           if (string.IsNullOrEmpty(message))
                                           {
                                               _logger.LogTrace("ignored empty message from {UserId} to {GroupName}", Context.User.GetUserId(), groupName);
                                               return;
                                           }

                                           await Clients.Group(groupName).SendAsync("MessageReceived", groupName, Context.User.GetName(), message);
                                       });
        }

        public async Task JoinGroup(string groupName)
        {
            await _apm.Trace("JoinGroup",
                             async () =>
                             {
                                 var userId      = Context.User.GetUserId();
                                 var communityId = new Guid(groupName);

                                 await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                                 await _queue.Join(communityId, userId);
                             });
        }

        [Authorize(Policy = Policies.JoinedQueueFromHubParameter0)]
        public async Task LeaveGroup(string groupName)
        {
            await _apm.Trace("LeaveGroup",
                             async () =>
                             {
                                 var userId      = Context.User.GetUserId();
                                 var communityId = new Guid(groupName);

                                 await _queue.Leave(communityId, userId);
                                 await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                             });
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            string userId = Context.User.GetUserId();
            _connections.AddConnection(userId, Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

            await RejoinQueue(userId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
                _logger.LogError(exception, "SignalR fatal disconnect");

            string userId = Context.User.GetUserId();
            _connections.RemoveConnection(userId, Context.ConnectionId);
            await _queue.LeaveAllQueues(userId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());

            await base.OnDisconnectedAsync(exception);
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
