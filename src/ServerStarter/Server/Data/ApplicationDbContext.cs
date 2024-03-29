﻿using System;
using Duende.IdentityServer.EntityFramework.Options;
using ServerStarter.Server.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
        {
            var versionString = "10.3.15-MariaDB-1";
            if (!ServerVersion.TryParse(versionString, out var serverVersion))
                throw new InvalidOperationException($"{versionString} can not be parsed to ServerVersion");

            options.UseMySql("Data Source=my.db", serverVersion);
        }
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
    class SqliteDbContext : ApplicationDbContext
    {
        public SqliteDbContext() : base(new DbContextOptions<SqliteDbContext>(),
                                        new OptionsManager<OperationalStoreOptions>(new OptionsFactory<OperationalStoreOptions>(new IConfigureOptions<OperationalStoreOptions>[0],
                                                                                        new IPostConfigureOptions<OperationalStoreOptions>[0])))
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=db.sqlite");
    }
}
