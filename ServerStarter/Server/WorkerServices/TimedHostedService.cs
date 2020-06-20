using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServerStarter.Server.WorkerServices
{
    public abstract class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> _logger;
        private readonly TimeSpan _interval;
        private          Timer                       _timer;

        public TimedHostedService(ILogger<TimedHostedService> logger, TimeSpan interval)
        {
            _logger = logger;
            _interval = interval;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                               _interval);

            return Task.CompletedTask;
        }

        protected abstract void DoWork(object state);

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}