using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServerStarter.Server.Models
{
    public class Community
    {
        [Key]
        public Guid Id { get; set; }

        public string                 Name                   { get; set; }
        public int                    MinimumPlayers         { get; set; }
        public int                    MaximumPlayers         { get; set; }
        public IList<CommunityServer> Servers                { get; set; }
        public int                    Order                  { get; set; }
        public byte[]                 HeaderImage            { get; set; }
        public string                 HeaderImageContentType { get; set; }
    }

    public class CommunityServer
    {
        [Key]
        public Guid Id { get; set; }

        public string Name  { get; set; }
        public string Ip    { get; set; }
        public int    Order { get; set; }
    }
}