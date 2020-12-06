using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace ServerStarter.Server.Settings
{
    public interface IHttpCacheSettings
    {
        IDictionary<string, CacheProfile> Profiles { get; set; }
    }

    public class HttpCacheSettings : IHttpCacheSettings
    {
        public IDictionary<string, CacheProfile> Profiles { get; set; }
    }
}
