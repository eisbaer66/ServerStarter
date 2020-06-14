﻿using System.Collections;
using System.Collections.Generic;

namespace ServerStarter.Shared
{
    public class Community
    {
        public string                 Name           { get; set; }
        public int                    CurrentPlayers { get; set; }
        public int                    WaitingPlayers { get; set; }
        public int                    MinimumPlayers { get; set; }
        public IList<CommunityServer> Servers        { get; set; }
    }

    public class CommunityServer
    {
        public string Name           { get; set; }
        public string Ip             { get; set; }
        public int    CurrentPlayers { get; set; }
    }
}