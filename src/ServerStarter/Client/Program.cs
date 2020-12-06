using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerStarter.Client.Hubs;
using ServerStarter.Shared;

namespace ServerStarter.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            //builder.Logging.SetMinimumLevel(LogLevel.Trace);

            builder.RootComponents.Add<App>("#app");

            var services = builder.Services;
            services.AddHttpClient("ServerStarter.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                             .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerStarter.ServerAPI"));

            services.AddApiAuthorization<RemoteAuthenticationState, UserAccount>()
                             .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, UserAccount, CustomAccountClaimsPrincipalFactory>();
            services.AddOidcAuthentication<RemoteAuthenticationState, UserAccount>(options =>
                                                                                            {
                                                                                                builder.Configuration.Bind("Local", options.ProviderOptions);
                                                                                                options.UserOptions.NameClaim = ClaimTypes.Name;
                                                                                            })
                             .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, UserAccount, CustomAccountClaimsPrincipalFactory>();
            services.AddSingleton(new QueueSettings
                                  {
                                      PlaySounds                  = true,
                                      AutomaticJoinEnabled        = true,
                                      AutomaticJoinDelayInSeconds = 30,
                                  });
            services.AddScoped<IQueueService, QueueService>();

            await builder.Build().RunAsync();
        }
    }
}
