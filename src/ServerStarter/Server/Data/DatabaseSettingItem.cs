using Microsoft.EntityFrameworkCore;

namespace ServerStarter.Server.Data
{
    public class DatabaseSettingItem
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string Meta { get; set; }

        //would be better as multiple implementations, but there is no build in support to read them from appsettings
        public void Configure(DbContextOptionsBuilder options)
        {
            switch (Provider)
            {
                case "MSSQL":
                    options.UseSqlServer(ConnectionString);
                    break;
                case "MySQL":
                    options.UseMySql(ConnectionString, mySqlOptions => mySqlOptions.ServerVersion(Meta));
                    break;
            }
        }
    }
}