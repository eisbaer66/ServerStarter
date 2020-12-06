using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using ServerStarter.Server.Settings;

namespace ServerStarter.Server.util
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddStaticFilesWithCache(this IApplicationBuilder app, IHttpCacheSettings settings)
        {
            var headerValue = CreateCacheControlHeaderValue(settings);
            return app.UseStaticFiles(new StaticFileOptions
                                      {
                                          OnPrepareResponse = context => { context.Context.Response.GetTypedHeaders().CacheControl = headerValue; }
                                      });
        }

        private static CacheControlHeaderValue CreateCacheControlHeaderValue(IHttpCacheSettings settings)
        {
            const int defaultDuration = 3600;
            if (!settings.Profiles.ContainsKey(CacheProfileName.StaticFiles))
                return new CacheControlHeaderValue
                       {
                           Public = true,
                           MaxAge = TimeSpan.FromSeconds(defaultDuration)
                       };

            var staticFilesCacheProfile = settings.Profiles[CacheProfileName.StaticFiles];
            var cacheIsPublic           = staticFilesCacheProfile.Location is null or ResponseCacheLocation.Any;
            var duration                = staticFilesCacheProfile.Duration ?? defaultDuration;
            var maxAge                  = TimeSpan.FromSeconds(duration);
            return new CacheControlHeaderValue()
                   {
                       Public = cacheIsPublic,
                       MaxAge = maxAge,
                   };
        }
    }
}