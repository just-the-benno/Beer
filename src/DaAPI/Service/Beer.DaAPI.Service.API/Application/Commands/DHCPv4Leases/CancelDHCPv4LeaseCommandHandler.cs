using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Infrastructure.InterfaceEngines;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv4Leases
{
    public class CancelDHCPv4LeaseCommandHandler : DHCPv4ScopeTriggerAwareCommandHandler, IRequestHandler<CancelDHCPv4LeaseCommand, Boolean>
    {
        private readonly ILogger<CancelDHCPv4LeaseCommandHandler> _logger;

        public CancelDHCPv4LeaseCommandHandler(
            IDHCPv4StorageEngine store,
            IServiceBus serviceBus,
            DHCPv4RootScope rootScope,
            ILogger<CancelDHCPv4LeaseCommandHandler> logger) : base(store,serviceBus,rootScope)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Boolean> Handle(CancelDHCPv4LeaseCommand request, CancellationToken cancellationToken)
        {
             _logger.LogDebug("Handle started");

            var lease = RootScope.GetLeaseById(request.LeaseId);
            if (lease == DHCPv4Lease.NotFound)
            {
                return false;
            }

            if(lease.IsCancelable() == false)
            {
                return false;
            }

            RootScope.CancelLease(lease);
            var result = await SaveRootAndTriggerEvents();
            return result;
        }
    }
}
