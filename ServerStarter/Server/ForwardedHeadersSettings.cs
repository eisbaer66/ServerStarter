using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.HttpOverrides;

namespace ServerStarter.Server
{
    public class ForwardedHeadersSettings
    {
        public int Headers { get; set; }
        public int Limit { get; set; }
        public IList<KnownNetworkSettings> KnownNetworks { get; set; }
    }

    public class KnownNetworkSettings
    {
        public string Prefix { get; set; }
        public int PrefixLength { get; set; }
    }
}