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
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var claim = GetClaim(user, JwtClaimTypes.Subject);
            if(claim == null)
                claim = GetClaim(user, ClaimTypes.NameIdentifier);

            var userId = new Guid(claim);
            return userId;
        }
    }
}