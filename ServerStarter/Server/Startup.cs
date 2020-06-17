using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OpenId.Steam;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.NetCoreAll;
using IdentityServer4.Hosting.LocalApiAuthentication;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using ServerStarter.Server.Data;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Hubs;
using ServerStarter.Server.Identity;
using ServerStarter.Server.Models;
using ServerStarter.Server.Services;
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
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                                                                                        Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<DbSet<ApplicationUser>>(c => c.GetService<ApplicationDbContext>().Users);

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();


            services.Configure<IdentityOptions>(options =>
                                                {
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

            services.AddSignalR();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddResponseCompression(opts =>
                                            {
                                                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
                                            });


            services.AddSingleton<HttpClient>(c => new HttpClient()
                                                   {
                                                       BaseAddress = new Uri("https://dev-8888.balancemod.tf/"),
                                                   });
            services.AddSingleton<ICommunityQueue, InMemoryCommunityQueue>();

            services.AddTransient<ICommunityRepository, CommunityRepository>();
            services.AddTransient<IServerInfoService, ServerInfoService>();
            services.AddTransient<IServerInfoQueries, ServerInfoQueries>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAllElasticApm(Configuration);
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