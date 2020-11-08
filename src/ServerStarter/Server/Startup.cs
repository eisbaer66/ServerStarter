using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OpenId.Steam;
using ClacksMiddleware.Extensions;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Helpers;
using Elastic.Apm.NetCoreAll;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using ServerStarter.Server.Data;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Data.Repositories.Queues;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.Identity;
using ServerStarter.Server.Identity.AuthPolicies;
using ServerStarter.Server.Identity.AuthPolicies.JoinedQueue;
using ServerStarter.Server.Models;
using ServerStarter.Server.Services;
using ServerStarter.Server.WorkerServices;
using ServerStarter.Server.ZarloAdapter;
using Zarlo.Stats;

namespace ServerStarter.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
                                                        {
                                                            var databaseSettings = new DatabaseSettings();
                                                            Configuration.GetSection("Database").Bind(databaseSettings);

                                                            var itemLogger = sp.GetRequiredService<ILogger<DatabaseSettingItem>>();
                                                            foreach (var item in databaseSettings.Databases.Values)
                                                            {
                                                                item.Logger = itemLogger;
                                                            }

                                                            DatabaseSettingItem database = databaseSettings.Get("ServerStarter");
                                                            database.Configure(options);
                                                        });
            services.AddTransient<DbSet<ApplicationUser>>(c => c.GetService<ApplicationDbContext>().Users);
            services.AddTransient<IQueryable<UserQueueStatistics>>(c => c.GetService<ApplicationDbContext>().UserQueueStatistics);

            var forwardedHeadersSettings = new ForwardedHeadersSettings();
            Configuration.Bind("ServerStarters:ForwardedHeaders", forwardedHeadersSettings);
            if (forwardedHeadersSettings.Headers != 0)
                services.Configure<ForwardedHeadersOptions>(options =>
                                                            {
                                                                options.ForwardedHeaders = (ForwardedHeaders)forwardedHeadersSettings.Headers;
                                                                options.ForwardLimit     = forwardedHeadersSettings.Limit;
                                                                foreach (var knownNetwork in forwardedHeadersSettings.KnownNetworks)
                                                                {
                                                                    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse(knownNetwork.Prefix), knownNetwork.PrefixLength));
                                                                }
                                                            });

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>();

            services.AddIdentityServer(opt => opt.UserInteraction.LoginUrl = "/api/SteamAuth")
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();


            services.Configure<IdentityOptions>(options =>
                                                {
                                                    options.SignIn.RequireConfirmedAccount = false;
                                                    options.SignIn.RequireConfirmedEmail = false;
                                                    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
                                                });
            services.AddAuthentication()
                .AddIdentityServerJwt()
                .AddSteam(options =>
                          {
                              options.ApplicationKey = Configuration["ServerStarters:SteamApiKey"];
                              options.Events.OnAuthenticated = async ctx =>
                                                               {
                                                                   ILogger<Startup> logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();
                                                                   logger.LogTrace("Steam OnAuthenticated {@SteamAuthUserPayload} {@SteamAuthTicket}", ctx.UserPayload, ctx.Ticket);

                                                                   ClaimsIdentity identity = ctx.Ticket.Principal.Identity as ClaimsIdentity;
                                                                   if (identity != null)
                                                                   {
                                                                       var profile = ctx.UserPayload.RootElement
                                                                                               .GetProperty(SteamAuthenticationConstants.Parameters.Response)
                                                                                               .GetProperty(SteamAuthenticationConstants.Parameters.Players)
                                                                                               .EnumerateArray()
                                                                                               .FirstOrDefault();

                                                                       if (profile.ValueKind == JsonValueKind.Object)
                                                                       {
                                                                           if (profile.TryGetProperty("steamid", out var steamid))
                                                                           {
                                                                               identity.AddClaim(new Claim(IcebearClaimTypes.SteamId, steamid.GetString(), ClaimValueTypes.String, ctx.Options.ClaimsIssuer));
                                                                           }
                                                                           if (profile.TryGetProperty("avatar", out var avatar))
                                                                           {
                                                                               identity.AddClaim(new Claim(IcebearClaimTypes.Avatar, avatar.GetString(), ClaimValueTypes.String, ctx.Options.ClaimsIssuer));
                                                                           }
                                                                       }
                                                                   }
                                                               };
                              options.Events.OnTicketReceived = async ctx => { };
                          });
            services.AddTransient<IProfileService, ProfileService>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>());

            services.AddSignalR();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddResponseCompression(opts =>
                                            {
                                                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
                                            });
            services.AddAuthorization(options =>
                                      {
                                          options.AddPolicy(Policies.JoinedQueueFromGroupName, 
                                                            policy => policy.Requirements.Add(new JoinedQueuePerParameterNameRequirement("groupName")));
                                          options.AddPolicy(Policies.JoinedQueueFromHubParameter0, 
                                                            policy => policy.Requirements.Add(new JoinedQueuePerHubParameterIndexRequirement(0)));
                                      });

            services.AddScoped<IAuthorizationHandler, JoinedQueuePerParameterNameHandler>();
            services.AddScoped<IAuthorizationHandler, JoinedQueuePerHubInvocationContextHandler>();


            services.AddSingleton<IMessaging, Messaging>();

            TimingSettings timingSettings = new TimingSettings();
            Configuration.Bind("ServerStarters:Timings", timingSettings);
            services.AddSingleton<ITimingSettings>(timingSettings);

            ManagedTimedHostedWorkerSettings managedTimedHostedWorkerSettings = new ManagedTimedHostedWorkerSettings();
            Configuration.Bind("ServerStarters:ManagedTimedHostedWorker", managedTimedHostedWorkerSettings);
            services.AddSingleton<IManagedTimedHostedWorkerSettings>(managedTimedHostedWorkerSettings);

            var settings = new ElasticSettings();
            Configuration.Bind("ServerStarters:Elastic", settings);
            services.AddSingleton<IElasticSettings>(settings);

            services.AddHttpClient(ServerInfoQueries.HttpClientName,
                                   c =>
                                   {
                                       c.BaseAddress = new Uri(Configuration["ServerStarters:ServerInfoBaseAddress"]);
                                       c.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                                   });
            services.AddScoped<CommunitiesHub>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<ICommunityState, CommunityState>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<CommunityQueueRepository, CommunityQueueRepository>();
            services.AddScoped<ICommunityQueueRepository>(c =>
                         {
                             var repository = c.GetRequiredService<CommunityQueueRepository>();
                             var dbContext  = c.GetRequiredService<ApplicationDbContext>();
                             return new GetAllQueuesCommunityQueueRepositoryCache(repository, dbContext);
                         });
            services.AddScoped<ICommunityQueueService, CommunityQueueService>();
            services.AddSingleton<CachingServerInfoQueries>(c =>
                                                      {
                                                          IMemoryCache cache = c.GetRequiredService<IMemoryCache>();
                                                          IServerInfoQueries queries = c.GetRequiredService<ServerInfoQueries>();
                                                          ILogger<CachingServerInfoQueries> logger = c.GetRequiredService<ILogger<CachingServerInfoQueries>>();
                                                          return new CachingServerInfoQueries(cache, timingSettings, queries, logger);
                                                      });
            services.AddSingleton<IServerInfoQueries>(c => c.GetRequiredService<CachingServerInfoQueries>());
            services.AddSingleton<IServerInfoCache, CachingServerInfoQueries>();
            services.AddSingleton<IHubConnectionSource<CommunitiesHub>, HubConnectionSource<CommunitiesHub>>();
            services.AddScoped<ICommunityUpdateService>(sp =>
                                                        {
                                                            ILogger<CommunityUpdateService> logger          = sp.GetRequiredService<ILogger<CommunityUpdateService>>();
                                                            IServerInfoCache                serverInfoCache = sp.GetRequiredService<IServerInfoCache>();
                                                            ICommunityRepository            repository      = sp.GetRequiredService<ICommunityRepository>();
                                                            ICommunityService               service         = sp.GetRequiredService<CommunityService>();
                                                            ICommunityServiceCache          cache           = sp.GetRequiredService<ICommunityServiceCache>();
                                                            return new CommunityUpdateService(logger, serverInfoCache, repository, service, timingSettings, cache);
                                                        });
            services.AddHostedService<CommunityQueueUpdateWorker>();
            services.AddHostedService<QueuedHostedService>();
            services.AddHostedService<CommunityCacheUpdateWorker>();

            services.AddTransient<CommunityRepository, CommunityRepository>();
            services.AddScoped<InMemoryCommunityRepositoryCache>(cp =>
                                                                 {
                                                                     ICommunityRepository repository = cp.GetRequiredService<CommunityRepository>();
                                                                     IMemoryCache         cache      = cp.GetRequiredService<IMemoryCache>();
                                                                     return new InMemoryCommunityRepositoryCache(repository, cache, timingSettings);
                                                                 });
            services.AddScoped<ICommunityRepositoryCache>(cp => cp.GetRequiredService<InMemoryCommunityRepositoryCache>());
            services.AddScoped<ICommunityRepository>(cp => cp.GetRequiredService<InMemoryCommunityRepositoryCache>());
            services.AddTransient<CommunityService>();
            services.AddScoped<InMemoryCommunityServiceCache>(cp =>
                                                              {
                                                                  CommunityService service = cp.GetRequiredService<CommunityService>();
                                                                  IMemoryCache     cache   = cp.GetRequiredService<IMemoryCache>();
                                                                  return new InMemoryCommunityServiceCache(service, cache, timingSettings);
                                                              });
            services.AddScoped<ICommunityServiceCache>(cp => cp.GetRequiredService<InMemoryCommunityServiceCache>());
            services.AddScoped<ICommunityService>(cp => cp.GetRequiredService<InMemoryCommunityServiceCache>());
            services.AddTransient<ServerInfoService>();
            services.AddTransient<IServerInfoService, ExceptionSwallowingServerInfoService>(sp =>
                                                                                            {
                                                                                                var service = sp.GetRequiredService<ServerInfoService>();
                                                                                                var logger  = sp.GetRequiredService<ILogger<ExceptionSwallowingServerInfoService>>();

                                                                                                return new ExceptionSwallowingServerInfoService(service, logger);
                                                                                            });
            services.AddTransient<ServerInfoQueries>();

            services.AddTransient<IHubApm<CommunitiesHub>, HubApm<CommunitiesHub>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.GnuTerryPratchett();
            app.UseForwardedHeaders();


            //Steam-Overlay sends multipart forms without setting ContentLength :/
            app.Use(async (ctx, next) =>
                    {
                        var isMultiPartFrom = ctx.Request.ContentType?.StartsWith("multipart/form-data") ?? false;
                        if (isMultiPartFrom)
                            ctx.Request.ContentLength ??= 0;
                        await next();
                    });

            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
            if (logger.IsEnabled(LogLevel.Trace))
                app.Use(async (ctx, next) =>
                    {
                        logger.LogTrace("incoming Request {RequestProtocol} {RequestScheme} {RequestHost} {RequestContentLength} bytes of {RequestContentType} {@RequestHeaders}", ctx.Request.Protocol, ctx.Request.Scheme, ctx.Request.Host, ctx.Request.ContentLength, ctx.Request.ContentType, ctx.Request.Headers);
                        await next();
                    });

            string scheme = Configuration["ServerStarters:Scheme"];
            if (!string.IsNullOrEmpty(scheme))
                app.Use(async (ctx, next) =>
                        {
                            if (ctx.Request.Scheme == "http")
                            {
                                ctx.Request.Scheme = scheme;
                            }

                            await next();
                        });
            string host = Configuration["ServerStarters:Host"];
            if (!string.IsNullOrEmpty(host))
                app.Use(async (ctx, next) =>
                        {
                            ctx.Request.Host = new HostString(host);
                            await next();
                        });

            IElasticSettings elasticSettings = app.ApplicationServices.GetRequiredService<IElasticSettings>();
            if (elasticSettings.AreSet() && elasticSettings.ApmEnabled)
            {
                app.UseAllElasticApm(Configuration);
                
                //don't create transactions for
                //- static files,
                //- blazor-framework
                //gonna get easier with 1.7 of Elastic.Apm.AspNetCore, see (TransactionIgnoreUrls)
                WildcardMatcher[] matchers = new string[]
                                                  {
                                                      "/VAADIN/*",
                                                      "/heartbeat*",
                                                      "/favicon.ico",
                                                      "*.js",
                                                      "*.css",
                                                      "*.jpg",
                                                      "*.jpeg",
                                                      "*.png",
                                                      "*.gif",
                                                      "*.webp",
                                                      "*.svg",
                                                      "*.woff",
                                                      "*.woff2",

                                                      "*.json",
                                                      "*.wav",
                                                      "/_framework/*",
                                                  }
                    .Select(WildcardMatcher.ValueOf)
                    .ToArray();
                string GetHeader(IDictionary<string, string> dict, string key)
                {
                    if (dict == null)
                        return null;
                    if (!dict.ContainsKey(key))
                        return null;
                    return dict[key].ToLowerInvariant();
                }
                Agent.AddFilter((ITransaction t) =>
                                {
                                    Request request    = t?.Context?.Request;
                                    //ignore transactions representing a WebSocket connection
                                    if (GetHeader(request?.Headers, "Connection") == "upgrade" &&
                                        GetHeader(request?.Headers, "Upgrade") == "websocket" &&
                                        GetHeader(t?.Custom, "icebear.HubApm") != "true")
                                        return null;

                                    var pathName       = request?.Url?.PathName;
                                    if (pathName != null && matchers.Any(m => m.Matches(pathName)))
                                        return null;
                                    return t;
                                });
            }

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<CommunitiesHub>("/hubs/communities");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
