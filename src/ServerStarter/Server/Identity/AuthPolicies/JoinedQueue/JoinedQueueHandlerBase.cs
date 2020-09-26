using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.Identity.AuthPolicies.JoinedQueue
{
    public abstract class JoinedQueueHandlerBase<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
    {
        protected readonly ILogger<JoinedQueuePerParameterNameHandler> Logger;
        private readonly   ICommunityQueueService                             _queue;

        public JoinedQueueHandlerBase(ILogger<JoinedQueuePerParameterNameHandler> logger, ICommunityQueueService queue)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queue = queue  ?? throw new ArgumentNullException(nameof(queue));
        }

        protected async Task HandleCommunity(AuthorizationHandlerContext context, string value, T requirement)
        {
            var  communityId = new Guid(value);
            string userId      = context.User.GetUserId();
            if (!await _queue.Contains(userId, communityId))
            {
                Logger.LogError("user {UserId} is not queued for community {CommunityId}", userId, communityId);
                return;
            }

            context.Succeed(requirement);
        }
    }
}