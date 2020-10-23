using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.ZarloAdapter;
using ServerStarter.Shared;

namespace ServerStarter.Server.Services
{
    public interface IServerInfoService
    {
        Task<ServerInfo> GetPlayersAsync(string ipAndPort, CancellationToken cancellationToken);
    }

    class ExceptionSwallowingServerInfoService : IServerInfoService
    {
        private readonly IServerInfoService                            _service;
        private readonly ILogger<ExceptionSwallowingServerInfoService> _logger;

        public ExceptionSwallowingServerInfoService(IServerInfoService service, ILogger<ExceptionSwallowingServerInfoService> logger)
        {
            _service     = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger  ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServerInfo> GetPlayersAsync(string ipAndPort, CancellationToken cancellationToken)
        {
            try
            {
                return await _service.GetPlayersAsync(ipAndPort, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error retrieving ServerInfos, returning empty ServerInfo");
                return new ServerInfo();
            }
        }
    }
}
