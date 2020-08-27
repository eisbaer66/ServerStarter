using System;
using Microsoft.AspNetCore.Authorization;

namespace ServerStarter.Server.Identity.AuthPolicies.JoinedQueue
{
    public class JoinedQueuePerHubParameterIndexRequirement : IAuthorizationRequirement
    {
        public JoinedQueuePerHubParameterIndexRequirement(int hubInvocationParameterIndex)
        {
            if (hubInvocationParameterIndex < 0) throw new ArgumentOutOfRangeException(nameof(hubInvocationParameterIndex));
            HubInvocationParameterIndex = hubInvocationParameterIndex;
        }

        public int HubInvocationParameterIndex { get; }
    }
}