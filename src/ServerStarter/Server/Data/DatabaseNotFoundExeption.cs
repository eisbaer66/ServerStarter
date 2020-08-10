using System;

namespace ServerStarter.Server.Data
{
    public class DatabaseNotFoundExeption : Exception
    {
        public string DatabaseName { get; }

        public DatabaseNotFoundExeption(string databaseName)
            : base("Database with name '" + databaseName + "' was not configured. check appsettings.json")
        {
            DatabaseName = databaseName;
        }
    }
}