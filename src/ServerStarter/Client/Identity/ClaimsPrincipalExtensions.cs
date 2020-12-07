using System.Linq;
using System.Security.Claims;

namespace ServerStarter.Client.Identity
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetName(this ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

            return claim?.Value ?? user.Identity.Name;
        }
    }
}