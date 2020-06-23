using System;
using System.Net.Http;
using System.Threading;
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

        public async Task<Server[]> GetServers(CancellationToken cancellationToken)
        {
            return (await GetJson<ServersResponse>("server/list", cancellationToken)).Servers;
        }

        public async Task<OnlinePlayerInfo[]> GetOnlinePlayers(CancellationToken cancellationToken)
        {
            return (await GetJson<OnlinePlayerResponse>("user/online", cancellationToken)).Users;
        }

        public async Task<T> GetJson<T>(string url, CancellationToken cancellationToken)
        {
            var response = await _client.GetAsync(url, cancellationToken);
            string json = await response.Content.ReadAsStringAsync();
            _logger.LogTrace("received {RawJson} from GET {Url} with {BaseAddress}", json, url, _client.BaseAddress);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}