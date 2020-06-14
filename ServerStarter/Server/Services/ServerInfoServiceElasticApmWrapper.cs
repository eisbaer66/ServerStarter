using System;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Logging;

namespace ServerStarter.Server.Services
{
    class ServerInfoServiceElasticApmWrapper : IServerInfoService
    {
        private readonly ILogger<ServerInfoServiceElasticApmWrapper> _logger;
        private readonly IServerInfoService                          _next;

        public ServerInfoServiceElasticApmWrapper(ILogger<ServerInfoServiceElasticApmWrapper> logger, IServerInfoService next)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _next   = next  ?? throw new ArgumentNullException(nameof(next));
        }
        public byte GetPlayerCount(string ipAndPort)
        {
            return Agent.Tracer.Capture("GetPlayerCount " + ipAndPort, ApiConstants.TypeExternal,
                                        () => _next.GetPlayerCount(ipAndPort));
        }

        public async Task<byte> GetPlayerCountAsync(string ipAndPort)
        {
            return await Agent.Tracer.CaptureAsync("GetPlayerCountAsync " + ipAndPort, ApiConstants.TypeExternal,
                                                   async () => await _next.GetPlayerCountAsync(ipAndPort));
        }
    }
}