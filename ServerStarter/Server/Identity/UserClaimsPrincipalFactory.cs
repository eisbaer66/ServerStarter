using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Identity
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public UserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (identity.IsAuthenticated)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name,           user.Name));
                identity.AddClaim(new Claim(IcebearClaimTypes.SteamId, user.SteamId));
                identity.AddClaim(new Claim(IcebearClaimTypes.Avatar,  user.AvatarUrl));
            }

            return identity;
        }
    }
}
