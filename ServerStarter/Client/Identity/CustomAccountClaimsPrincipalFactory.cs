using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Extensions.Logging;
using ServerStarter.Server;

namespace ServerStarter.Client
{
    public class CustomAccountClaimsPrincipalFactory : AccountClaimsPrincipalFactory<UserAccount>
    {
        private readonly ILogger<CustomAccountClaimsPrincipalFactory> _logger;

        public CustomAccountClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor, ILogger<CustomAccountClaimsPrincipalFactory> logger) : base(accessor)
        {
            _logger = logger;
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(UserAccount account, RemoteAuthenticationUserOptions options)
        {
            var principal = await base.CreateUserAsync(account, options);
            
            if (principal.Identity.IsAuthenticated)
            {
                foreach (var value in account.AuthenticationMethod)
                {
                    ((ClaimsIdentity)principal.Identity)
                        .AddClaim(new Claim("amr", value));
                }

                if (!string.IsNullOrEmpty(account.SteamId))
                    ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(IcebearClaimTypes.SteamId, account.SteamId));
                if (!string.IsNullOrEmpty(account.AvatarUrl))
                    ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(IcebearClaimTypes.Avatar, account.AvatarUrl));
                if (!string.IsNullOrEmpty(account.Name))
                {
                    ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.Name, account.Name));
                    ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("name", account.Name));
                }
            }

            return principal;
        }
    }
}