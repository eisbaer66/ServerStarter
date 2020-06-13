using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Models;

namespace ServerStarter.Server
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
        }
    }
}
