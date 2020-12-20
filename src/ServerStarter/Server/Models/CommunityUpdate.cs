using System;
using System.Collections.Generic;
using System.Linq;
using ServerStarter.Server.ZarloAdapter;
using ServerStarter.Shared;

namespace ServerStarter.Server.Models
{
    public class CommunityUpdate
    {
        public Guid                         Id             { get; set; }
        public string                       Name           { get; set; }
        public int                          CurrentPlayers { get; set; }
        public int                          WaitingPlayers { get; set; }
        public int                          MinimumPlayers { get; set; }
        public IList<CommunityUpdateServer> Servers        { get; set; }
        public IList<CommunityUpdatePlayer> QueuedPlayers  { get; set; }
        public DateTime                     Updated        { get; set; }


        public Shared.Community ToDto(string iconUrl)
        {
            return new Shared.Community
                   {
                       Id             = Id,
                       Name           = Name,
                       IconUrl        = iconUrl,
                       CurrentPlayers = CurrentPlayers,
                       WaitingPlayers = WaitingPlayers,
                       MinimumPlayers = MinimumPlayers,
                       Servers        = Servers.Select(s => s.ToDto()).ToList(),
                       QueuedPlayers  = QueuedPlayers.Select(p => p.ToCommunityPlayerDto()).ToList(),
                       Updated        = DateTime.UtcNow,
                   };
        }
        protected bool Equals(CommunityUpdate other)
        {
            return Id.Equals(other.Id)                                &&
                   Name           == other.Name                       &&
                   CurrentPlayers == other.CurrentPlayers             &&
                   WaitingPlayers == other.WaitingPlayers             &&
                   MinimumPlayers == other.MinimumPlayers             &&
                   Servers.EqualsBySelector(other.Servers, s => s.Ip) &&
                   QueuedPlayers.EqualsByIndex(other.QueuedPlayers, p => p.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CommunityUpdate)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, CurrentPlayers, WaitingPlayers, MinimumPlayers, Servers, QueuedPlayers);
        }
    }

    public class CommunityUpdateServer
    {
        public string                       Name              { get; set; }
        public string                       Ip                { get; set; }
        public int                          CurrentPlayers    { get; set; }
        public IList<CommunityUpdatePlayer> Players           { get; set; }
        public int                          MaxPlayers        { get; set; }
        public bool                         ConsideredFull    { get; set; }
        public bool                         PreferredForQueue { get; set; }


        public Shared.CommunityServer ToDto()
        {
            return new Shared.CommunityServer
                   {
                       Name              = Name,
                       Ip                = Ip,
                       CurrentPlayers    = CurrentPlayers,
                       Players           = Players.Select(p => p.ToServerPlayerDto()).ToList(),
                       MaxPlayers        = MaxPlayers,
                       ConsideredFull    = ConsideredFull,
                       PreferredForQueue = PreferredForQueue,
                   };
        }

        protected bool Equals(CommunityUpdateServer other)
        {
            return Name              == other.Name              &&
                   Ip                == other.Ip                &&
                   CurrentPlayers    == other.CurrentPlayers    &&
                   MaxPlayers        == other.MaxPlayers        &&
                   ConsideredFull    == other.ConsideredFull    &&
                   PreferredForQueue == other.PreferredForQueue &&
                   Players.EqualsBySelector(other.Players, p => p.SteamId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CommunityUpdateServer)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Ip, CurrentPlayers, Players, MaxPlayers, ConsideredFull, PreferredForQueue);
        }
    }

    public class CommunityUpdatePlayer
    {
        public long SteamId { get; set; }
        public string Name { get; set; }


        public static IList<CommunityUpdatePlayer> From(IList<Player> players)
        {
            return players.Select(From).ToList();
        }

        public static CommunityUpdatePlayer From(Player player)
        {
            return new CommunityUpdatePlayer
                   {
                       SteamId = player.SteamId,
                   };
        }

        public CommunityPlayer ToCommunityPlayerDto()
        {
            return new CommunityPlayer
                   {
                       Name = Name,
                   };
        }

        public ServerPlayer ToServerPlayerDto()
        {
            return new ServerPlayer
                   {
                       SteamId = SteamId,
                   };
        }

        protected bool Equals(CommunityUpdatePlayer other)
        {
            return SteamId == other.SteamId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CommunityUpdatePlayer)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, SteamId);
        }
    }
}