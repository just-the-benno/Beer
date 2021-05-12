using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Services;
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
    public class CleanupDatabaseTimerHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<LeaseTimerHostedService> _logger;
        private Timer _timer;

        private static Boolean _operationInProgress = false;

        public CleanupDatabaseTimerHostedService(IServiceProvider services,
            ILogger<LeaseTimerHostedService> logger)
        {
            this._services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Random random = new Random();

            _logger.LogInformation("CleanupDatabaseTimerHostedService Service running.");

            _timer = new Timer((x) => { try { DoWork(x); } catch { } }, null, TimeSpan.FromMilliseconds(10_000 + random.Next(200, 600)),
                TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        private async void DoWork(object _)
        {
            _logger.LogInformation("Database cleanup timer intervall started. Checking for leases and data to delete");

            if (_operationInProgress == true)
            {
                _logger.LogInformation("another cleanup process hasn't finished yet. Canceling this cycle");
                return;
            }

            _operationInProgress = true;
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var serverPropertiesResolver = scope.ServiceProvider.GetRequiredService<IDHCPv6ServerPropertiesResolver>();

                    DateTime now = DateTime.UtcNow;
                    DateTime leaseThreshold = now - serverPropertiesResolver.GetLeaseLifeTime();
                    DateTime handledEventThreshold = now - serverPropertiesResolver.GetHandledLifeTime();

                    {
                        var rootScope = scope.ServiceProvider.GetRequiredService<DHCPv6RootScope>();
                        var storageEngine = scope.ServiceProvider.GetRequiredService<IDHCPv6StorageEngine>();

                        rootScope.DropUnusedLeasesOlderThan(leaseThreshold);
                        await storageEngine.DeleteLeaseRelatedEventsOlderThan(leaseThreshold);

                        await storageEngine.DeletePacketHandledEventsOlderThan(handledEventThreshold);
                        await storageEngine.DeletePacketHandledEventMoreThan(serverPropertiesResolver.GetMaximumHandledCounter());
                    }

                    {
                        var rootScope = scope.ServiceProvider.GetRequiredService<DHCPv4RootScope>();
                        var storageEngine = scope.ServiceProvider.GetRequiredService<IDHCPv4StorageEngine>();

                        rootScope.DropUnusedLeasesOlderThan(leaseThreshold);

                        //handled by previous part as well
                        //await storageEngine.DeleteLeaseRelatedEventsOlderThan(leaseThreshold);

                        //await storageEngine.DeletePacketHandledEventsOlderThan(handledEventThreshold);
                        //await storageEngine.DeletePacketHandledEventMoreThan(serverPropertiesResolver.GetMaximumHandledCounter());
                    }

                    _logger.LogInformation("Database cleanup intervall finished");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "clean up intervall finished with error");
            }
            finally
            {
                _operationInProgress = false;
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database clean timer is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
