using System;
using System.Collections;
using System.Collections.Generic;

namespace ServerStarter.Shared
{
    public class Community
    {
        public Guid                   Id             { get; set; }
        public string                 Name           { get; set; }
        public int                    CurrentPlayers { get; set; }
        public int                    WaitingPlayers { get; set; }
        public int                    MinimumPlayers { get; set; }
        public IList<CommunityServer> Servers        { get; set; }

        protected bool Equals(Community other)
        {
            return Id.Equals(other.Id)                    &&
                   Name           == other.Name           &&
                   CurrentPlayers == other.CurrentPlayers &&
                   WaitingPlayers == other.WaitingPlayers &&
                   MinimumPlayers == other.MinimumPlayers &&
                   Servers.EqualsByIndex(other.Servers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Community)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, CurrentPlayers, WaitingPlayers, MinimumPlayers, Servers);
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

        protected bool Equals(CommunityServer other)
        {
            return Name            == other.Name            &&
                   Ip              == other.Ip              &&
                   CurrentPlayers  == other.CurrentPlayers  &&
                   MaxPlayers      == other.MaxPlayers      &&
                   ConsideredFull  == other.ConsideredFull  &&
                   PreferredForQueue == other.PreferredForQueue &&
                   Players.EqualsByIndex(other.Players);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CommunityServer) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Ip, CurrentPlayers, Players, MaxPlayers, ConsideredFull, PreferredForQueue);
        }
    }

    public class ServerPlayer
    {
        public long SteamId { get; set; }

        protected bool Equals(ServerPlayer other)
        {
            return SteamId == other.SteamId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerPlayer) obj);
        }

        public override int GetHashCode()
        {
            return SteamId.GetHashCode();
        }
    }
}