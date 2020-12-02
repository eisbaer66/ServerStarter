using ServerStarter.Server.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ServerStarter.Server.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions                  options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunityQueue> CommunitiesQueues { get; set; }
        public DbSet<UserQueueStatistics> UserQueueStatistics { get; set; }
    }
    class MySqlDbContext : ApplicationDbContext
    {
        public MySqlDbContext() : base(new DbContextOptions<MySqlDbContext>(),
                                       new OptionsManager<OperationalStoreOptions>(new OptionsFactory<OperationalStoreOptions>(new IConfigureOptions<OperationalStoreOptions>[0],
                                                                                                                               new IPostConfigureOptions<OperationalStoreOptions>[0])))
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("Data Source=my.db", ServerVersion.FromString("10.3.15-MariaDB-1"));
    }
    class MsSqlDbContext : ApplicationDbContext
    {
        public MsSqlDbContext() : base(new DbContextOptions<MsSqlDbContext>(),
                                       new OptionsManager<OperationalStoreOptions>(new OptionsFactory<OperationalStoreOptions>(new IConfigureOptions<OperationalStoreOptions>[0],
                                                                                                                               new IPostConfigureOptions<OperationalStoreOptions>[0])))
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=aspnet-ServerStarter.Server-EA1545A0-EB79-480A-9D38-989950E652AB;Integrated Security=SSPI;");
    }
}
