using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ServerStarter.Server.Data
{
    public class DatabaseSettingItem
    {
        public string                       Provider         { get; set; }
        public string                       ConnectionString { get; set; }
        public string                       Meta             { get; set; }
        public bool                         RetryOnFailure   { get; set; }
        public int                          MaxRetryAttempts { get; set; }
        public TimeSpan                     MaxRetryDelay    { get; set; }
        public ILogger<DatabaseSettingItem> Logger           { get; set; }

        //would be better as multiple implementations, but there is no build in support to read them from appsettings
        public void Configure(DbContextOptionsBuilder options)
        {
            switch (Provider)
            {
                case "MSSQL":
                    options.UseSqlServer(ConnectionString,
                                         builder =>
                                         {
                                             builder.RetryOnFailure(this);
                                         });
                    break;
                case "MySQL":
                    if (!ServerVersion.TryParse(Meta, out var serverVersion))
                        throw new InvalidOperationException($"{Meta} can not be parsed to ServerVersion");

                    options.UseMySql(ConnectionString,
                                     serverVersion,
                                     builder =>
                                     {
                                         builder.RetryOnFailure(this);
                                     });
                    break;
                case "SQLite":
                    options.UseSqlite(ConnectionString);
                    break;
            }
        }
    }
}