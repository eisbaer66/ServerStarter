using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ServerStarter.Server.Data
{
    public class DatabaseSettings
    {
        public IDictionary<string, DatabaseSettingItem> Databases { get; set; }

        public DatabaseSettingItem Get(string key)
        {
            if (!Databases.ContainsKey(key))
            {
                throw new DatabaseNotFoundExeption(key);
            }

            return Databases[key];
        }
    }
}