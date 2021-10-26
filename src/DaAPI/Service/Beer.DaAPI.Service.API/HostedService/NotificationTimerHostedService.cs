using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.HostedService
{
    public class NotificationTimerHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<LeaseTimerHostedService> _logger;
        private Timer _timer;

        public NotificationTimerHostedService(IServiceProvider services,
            ILogger<LeaseTimerHostedService> logger)
        {
            this._services = services;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationTimerHostedService Service running.");

            _timer = new Timer((x) => { try { DoWork(x); } catch { } }, null, TimeSpan.FromSeconds(45),
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object _)
        {
            _logger.LogInformation("Lease Timer intervall started. Checking for expired leases");
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var serviceBus = scope.ServiceProvider.GetRequiredService<IServiceBus>();
                    await serviceBus.Publish(new NewTriggerHappendMessage(new [] { new TimeIntervallTrigger() }));

                    _logger.LogInformation("TimeIntervallTrigger started");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "clean up intervall finished with error");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationTimerHostedService Timer is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
