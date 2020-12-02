using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ServerStarter.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            //builder.Logging.SetMinimumLevel(LogLevel.Trace);

            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("ServerStarter.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerStarter.ServerAPI"));

            builder.Services.AddApiAuthorization<RemoteAuthenticationState, UserAccount>()
                   .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, UserAccount, CustomAccountClaimsPrincipalFactory>();
            builder.Services.AddOidcAuthentication<RemoteAuthenticationState, UserAccount>(options =>
                                                   {
                                                       builder.Configuration.Bind("Local", options.ProviderOptions);
                                                       options.UserOptions.NameClaim = ClaimTypes.Name;
                                                   })
                   .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, UserAccount, CustomAccountClaimsPrincipalFactory>();

            await builder.Build().RunAsync();
        }
    }
}
