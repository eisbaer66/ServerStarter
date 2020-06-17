using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Areas.Identity.Pages.Account;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SteamAuthController : ControllerBase
    {
        private readonly ILogger<SteamAuthController>   _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser>   _userManager;

        public SteamAuthController(ILogger<SteamAuthController>   logger,
                                   SignInManager<ApplicationUser> signInManager,
                                   UserManager<ApplicationUser>   userManager)
        {
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager   = userManager   ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public async Task<IActionResult> Get(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // copied from ExternalLogin.OnPost to skip selection of internal/extenal login
            // Request a redirect to the external login provider.
            string provider    = "Steam";
            var    redirectUrl = Url.Action("Callback", new { returnUrl });
            var    properties  = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {remoteError}", remoteError);
                return base.BadRequest("Error from external provider: " + remoteError);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Error loading external login information.");
                return base.BadRequest("Error loading external login information.");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ExternalLoginModel.InputModel input = new ExternalLoginModel.InputModel();
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    input.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                }
                return await CreateLogin(returnUrl, info.ProviderDisplayName, input);
            }
        }

        public async Task<IActionResult> CreateLogin(string returnUrl, string infoProviderDisplayName, ExternalLoginModel.InputModel input)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Error loading external login information during confirmation.");
                return base.BadRequest("Error loading external login information during confirmation.");
            }

            if (ModelState.IsValid)
            {
                long steamId = 0;
                if (info.Principal.HasClaim(c => c.Type == IcebearClaimTypes.SteamId))
                {
                    string steamIdRaw = info.Principal.FindFirst(IcebearClaimTypes.SteamId).Value;
                    if (!long.TryParse(steamIdRaw, out steamId))
                    {
                        _logger.LogError("couldn't read {SteamId} as long", steamIdRaw);

                        return base.BadRequest("invalid steamid");
                    }
                }

                string avatar = string.Empty;
                if (info.Principal.HasClaim(c => c.Type == IcebearClaimTypes.Avatar))
                {
                    avatar = info.Principal.FindFirst(IcebearClaimTypes.Avatar).Value;
                }

                if (!info.Principal.HasClaim(c => c.Type == ClaimTypes.Name))
                {
                    _logger.LogError("token contains no name");
                    return base.BadRequest("token contains no name");
                }
                string name = info.Principal.FindFirst(ClaimTypes.Name).Value;

                var user = new ApplicationUser
                {
                    UserName = name,
                    Name = name,
                    SteamId = steamId,
                    AvatarUrl = avatar,
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        
                        await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Ok();
        }
    }
}