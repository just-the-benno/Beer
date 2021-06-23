using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv6Leases
{
    public class CancelDHCPv6LeaseCommandHandler : DHCPv6ScopeTriggerAwareCommandHandler, IRequestHandler<CancelDHCPv6LeaseCommand, Boolean>
    {
        private readonly ILogger<CancelDHCPv6LeaseCommandHandler> _logger;

        public CancelDHCPv6LeaseCommandHandler(
            IDHCPv6StorageEngine storageEngine,
            IServiceBus serviceBus,
            DHCPv6RootScope rootScope,
            ILogger<CancelDHCPv6LeaseCommandHandler> logger) : base(storageEngine, serviceBus, rootScope)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Boolean> Handle(CancelDHCPv6LeaseCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var lease = RootScope.GetLeaseById(request.LeaseId);
            if (lease == DHCPv6Lease.NotFound)
            {
                return false;
            }

            if (lease.IsCancelable() == false)
            {
                return false;
            }

            RootScope.CancelLease(lease);
            var result = await SaveRootAndTriggerEvents();
            return result;
        }
    }
}
