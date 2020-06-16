using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Zarlo.Stats.Data;
using Zarlo.Stats.Data.Responses;

namespace Zarlo.Stats
{
    public class ServerInfoQueries : IServerInfoQueries
    {
        private readonly ILogger<ServerInfoQueries> _logger;
        private readonly HttpClient                 _client;

        public ServerInfoQueries(ILogger<ServerInfoQueries> logger, HttpClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<Server[]> GetServers()
        {
            return (await GetJson<ServersResponse>("api/server/list")).Servers;
        }

        public async Task<OnlinePlayerInfo[]> GetOnlinePlayers()
        {
            return (await GetJson<OnlinePlayerResponse>("api/user/online")).Users;
        }

        public async Task<T> GetJson<T>(string url)
        {
            string json = await _client.GetStringAsync(url);
            _logger.LogTrace("received {RawJson} from GET {Url} with {BaseAddress}", json, url, _client.BaseAddress);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}