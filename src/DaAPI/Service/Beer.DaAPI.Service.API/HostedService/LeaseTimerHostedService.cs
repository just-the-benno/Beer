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
    public class LeaseTimerHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<LeaseTimerHostedService> _logger;
        private Timer _timer;

        public LeaseTimerHostedService(IServiceProvider services,
            ILogger<LeaseTimerHostedService> logger)
        {
            this._services = services;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LeaseTimerHostedService Service running.");

            _timer = new Timer((x) => { try { DoWork(x); } catch { } }, null, TimeSpan.FromSeconds(30),
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

                    {
                        var eventStore = scope.ServiceProvider.GetRequiredService<IDHCPv6EventStore>();
                        var rootScope = scope.ServiceProvider.GetRequiredService<DHCPv6RootScope>();

                        Int32 changeAmount = rootScope.CleanUpLeases();
                        _logger.LogInformation("{ChangeAmount} expired dhcpv6 leases found", changeAmount);

                        await eventStore.Save(rootScope, 20);

                        if (changeAmount > 0)
                        {
                            var triggers = rootScope.GetTriggers();
                            if (triggers.Any() == true)
                            {
                                await serviceBus.Publish(new NewTriggerHappendMessage(triggers));
                                rootScope.ClearTriggers();
                            }
                        }
                    }

                    {

                        var eventStore = scope.ServiceProvider.GetRequiredService<IDHCPv4EventStore>();
                        var rootScope = scope.ServiceProvider.GetRequiredService<DHCPv4RootScope>();

                        Int32 changeAmount = rootScope.CleanUpLeases();
                        _logger.LogInformation("{ChangeAmount} expired dhcpv4 leases found", changeAmount);

                        await eventStore.Save(rootScope, 20);

                        if (changeAmount > 0)
                        {
                            var triggers = rootScope.GetTriggers();
                            if (triggers.Any() == true)
                            {
                                await serviceBus.Publish(new NewTriggerHappendMessage(triggers));
                                rootScope.ClearTriggers();
                            }
                        }
                    }

                    _logger.LogInformation("Lease Timer intervall finished");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "clean up intervall finished with error");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Lease Timer is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
