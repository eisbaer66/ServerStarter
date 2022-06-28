using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Identity
{
    public class ProfileService : ProfileService<ApplicationUser>
    {
        public ProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) : base(userManager, claimsFactory)
        {
        }

        public ProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, ILogger<ProfileService<ApplicationUser>> logger) : base(userManager, claimsFactory, logger)
        {
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);

            var steamIdClaims = context.Subject.FindAll(IcebearClaimTypes.SteamId);
            context.IssuedClaims.AddRange(steamIdClaims);

            var avatarClaims = context.Subject.FindAll(IcebearClaimTypes.Avatar);
            context.IssuedClaims.AddRange(avatarClaims);

            var nameClaims = context.Subject.FindAll(ClaimTypes.Name);
            context.IssuedClaims.AddRange(nameClaims);
        }
    }
}
