using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace ServerStarter.Server.Hubs
{
    public interface IHubConnectionSource<T> where T : Hub
    {
        void AddConnection(string    userId, string connectionId);
        void RemoveConnection(string userId, string connectionId);
        bool UsersConnected();
    }

    public class HubConnectionSource<T> : IHubConnectionSource<T> where T : Hub
    {
        private readonly IDictionary<string, HashSet<string>> _connections = new Dictionary<string, HashSet<string>>();
        
        public bool UsersConnected()
        {
            return _connections.Count > 0;
        }
        
        public void AddConnection(string userId, string connectionId)
        {
            if (!_connections.ContainsKey(userId))
                _connections.Add(userId, new HashSet<string>());
            _connections[userId].Add(connectionId);
        }

        public void RemoveConnection(string userId, string connectionId)
        {
            if (!_connections.ContainsKey(userId))
                return;
            if (!_connections[userId].Contains(connectionId))
                return;

            _connections[userId].Remove(connectionId);
            if (_connections[userId].Count != 0)
                return;

            _connections.Remove(userId);
        }
    }
}