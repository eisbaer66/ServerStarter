using System;

namespace ServerStarter.Shared
{
    public class Region
    {
        public Guid     Id             { get; set; }
        public string   Name           { get; set; }
        public int      CurrentPlayers { get; set; }
        public int      WaitingPlayers { get; set; }
        public DateTime Updated        { get; set; }

        protected bool Equals(Community other)
        {
            return Id.Equals(other.Id)                    &&
                   Name           == other.Name           &&
                   CurrentPlayers == other.CurrentPlayers &&
                   WaitingPlayers == other.WaitingPlayers;
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
            return HashCode.Combine(Id, Name, CurrentPlayers, WaitingPlayers);
        }
    }
}