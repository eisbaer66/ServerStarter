using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ServerStarter.Server.Data
{
    public static class DbContextOptionsBuilderExtensions
    {

        public static MySqlDbContextOptionsBuilder RetryOnFailure(this MySqlDbContextOptionsBuilder builder, DatabaseSettingItem setting)
        {
            return RetryOnSqlFailure(builder, builder.EnableRetryOnFailure, builder.EnableRetryOnFailure, setting);
        }

        public static SqlServerDbContextOptionsBuilder RetryOnFailure(this SqlServerDbContextOptionsBuilder builder, DatabaseSettingItem setting)
        {
            return RetryOnSqlFailure(builder, builder.EnableRetryOnFailure, builder.EnableRetryOnFailure, setting);
        }

        private static T RetryOnSqlFailure<T>(T builder, Func<int, TimeSpan, ICollection<int>, T> withValues, Func<T> defaults, DatabaseSettingItem setting)
        {
            if (!setting.RetryOnFailure)
                return builder;

            if (setting.MaxRetryAttempts > 0 && setting.MaxRetryDelay > TimeSpan.Zero)
                return withValues(setting.MaxRetryAttempts, setting.MaxRetryDelay, null);
            if (setting.MaxRetryAttempts > 0 || setting.MaxRetryDelay > TimeSpan.Zero)
            {
                setting.Logger.LogWarning("Both MaxRetryAttempts and MaxRetryDelay must be set. using Defaults for both (ifaik: MaxRetryAttempts: 6 MaxRetryDelay: 30seconds");
                return defaults();
            }
            return defaults();
        }
    }
}