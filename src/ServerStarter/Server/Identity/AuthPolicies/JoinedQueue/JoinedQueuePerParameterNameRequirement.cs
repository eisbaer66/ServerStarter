using System;
using Microsoft.AspNetCore.Authorization;

namespace ServerStarter.Server.Identity.AuthPolicies.JoinedQueue
{
    public class JoinedQueuePerParameterNameRequirement : IAuthorizationRequirement
    {
        public JoinedQueuePerParameterNameRequirement(string parameterName)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        }

        public string ParameterName { get; }
    }
}