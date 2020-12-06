using System;
using System.Collections;
using System.Collections.Generic;

namespace ServerStarter.Shared
{
    public class Community
    {
        public        Guid                   Id             { get; set; }
        public        string                 Name           { get; set; }
        public        int                    CurrentPlayers { get; set; }
        public        int                    WaitingPlayers { get; set; }
        public        int                    MinimumPlayers { get; set; }
        public        IList<CommunityServer> Servers        { get; set; }
        public        IList<CommunityPlayer> QueuedPlayers  { get; set; }
        public        DateTime               Updated        { get; set; }
    }

    public class CommunityPlayer
    {
        public string Name { get; set; }

        protected bool Equals(CommunityPlayer other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CommunityPlayer) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public class CommunityServer
    {
        public string              Name              { get; set; }
        public string              Ip                { get; set; }
        public int                 CurrentPlayers    { get; set; }
        public IList<ServerPlayer> Players           { get; set; }
        public int                 MaxPlayers        { get; set; }
        public bool                ConsideredFull    { get; set; }
        public bool                PreferredForQueue { get; set; }
    }

    public class ServerPlayer
    {
        public long SteamId { get; set; }
    }
}