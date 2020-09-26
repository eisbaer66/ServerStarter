using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.Identity.AuthPolicies.JoinedQueue
{
    public class JoinedQueuePerHubInvocationContextHandler : JoinedQueueHandlerBase<JoinedQueuePerHubParameterIndexRequirement>
    {
        public JoinedQueuePerHubInvocationContextHandler(ILogger<JoinedQueuePerParameterNameHandler> logger, ICommunityQueueService queue) : base(logger, queue)
        {
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext                context,
                                                       JoinedQueuePerHubParameterIndexRequirement requirement)
        {
            var mvcContext = context.Resource as HubInvocationContext;

            if (mvcContext == null)
            {
                Logger.LogError("could not find HubInvocationContext");
                return Task.CompletedTask;
            }

            string value = mvcContext.HubMethodArguments[requirement.HubInvocationParameterIndex] as string;
            return HandleCommunity(context, value, requirement);
        }
    }
}