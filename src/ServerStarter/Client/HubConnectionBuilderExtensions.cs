using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace ServerStarter.Client
{
    public static class HubConnectionBuilderExtensions
    {
        public static IHubConnectionBuilder WithDefaultConfig(this IHubConnectionBuilder builder, IAccessTokenProvider accessTokenProvider, NavigationManager navigationManager)
        {
            void ConfigureHttpConnection(HttpConnectionOptions options)
            {
                options.AccessTokenProvider = async () =>
                                              {
                                                  var         result = await accessTokenProvider.RequestAccessToken();
                                                  AccessToken token;
                                                  bool        tryGetToken = result.TryGetToken(out token);
                                                  if (!tryGetToken)
                                                      return null;
                                                  return token.Value;
                                              };
            }

            return builder
                .WithUrl(navigationManager.ToAbsoluteUri("/hubs/communities"), ConfigureHttpConnection)
                .WithAutomaticReconnect(new[]
                                        {
                                            TimeSpan.Zero,
                                            TimeSpan.FromSeconds(1),
                                            TimeSpan.FromSeconds(2),
                                            TimeSpan.FromSeconds(2),
                                            TimeSpan.FromSeconds(5),
                                            TimeSpan.FromSeconds(10),
                                            TimeSpan.FromSeconds(10),
                                            TimeSpan.FromSeconds(30),
                                            TimeSpan.FromSeconds(60),
                                        });
        }
    }
}