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
        public const     string                     HttpClientName = "ZarloServerInfoApi";
        private readonly ILogger<ServerInfoQueries> _logger;
        private readonly IHttpClientFactory         _clientFactory;

        public ServerInfoQueries(ILogger<ServerInfoQueries> logger, IHttpClientFactory clientFactory)
        {
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
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
            var client   = _clientFactory.CreateClient(HttpClientName);
            var response = await client.GetAsync(url, cancellationToken);
            var json     = await response.Content.ReadAsStringAsync();
            _logger.LogTrace("received {RawJson} from GET {Url} with {BaseAddress}", json, url, client.BaseAddress);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}