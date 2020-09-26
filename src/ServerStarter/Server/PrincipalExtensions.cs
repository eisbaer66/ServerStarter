using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;

namespace ServerStarter.Server
{
    public static class PrincipalExtensions
    {
        public static string GetClaim(this ClaimsPrincipal user, string claimType)
        {
            return user.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        public static string GetName(this ClaimsPrincipal user) //TODO figure out how to set NameClaimType correctly
        {
            return GetClaim(user, ClaimTypes.Name);
        }
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return GetClaim(user, JwtClaimTypes.Subject) ?? GetClaim(user, ClaimTypes.NameIdentifier);
        }
    }
}