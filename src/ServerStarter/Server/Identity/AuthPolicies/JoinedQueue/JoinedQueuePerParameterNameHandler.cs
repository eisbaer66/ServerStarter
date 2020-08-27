using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Services;

namespace ServerStarter.Server.Identity.AuthPolicies.JoinedQueue
{
    public class JoinedQueuePerParameterNameHandler : JoinedQueueHandlerBase<JoinedQueuePerParameterNameRequirement>
    {
        public JoinedQueuePerParameterNameHandler(ILogger<JoinedQueuePerParameterNameHandler> logger, ICommunityQueue queue) : base(logger, queue)
        {
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext            context,
                                                       JoinedQueuePerParameterNameRequirement requirement)
        {
            var mvcContext = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;

            if (mvcContext == null)
            {
                Logger.LogError("could not find AuthorizationFilterContext");
                return Task.CompletedTask;
            }

            string value = mvcContext.RouteData.Values[requirement.ParameterName] as string;
            return HandleCommunity(context, value, requirement);
        }
    }
}