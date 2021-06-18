using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv4Scopes
{
    public class CreateDHCPv4ScopeCommandHandler : ManipulateDHCPv4ScopeCommandHandler, IRequestHandler<CreateDHCPv4ScopeCommand, Guid?>
    {
        private readonly ILogger<CreateDHCPv4ScopeCommandHandler> _logger;

        public CreateDHCPv4ScopeCommandHandler(
            IDHCPv4StorageEngine store, DHCPv4RootScope rootScope,
            IServiceBus serviceBus,
            ILogger<CreateDHCPv4ScopeCommandHandler> logger) : base(store, serviceBus, rootScope)
        {
            _logger = logger;
        }

        public async Task<Guid?> Handle(CreateDHCPv4ScopeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            Guid id = Guid.NewGuid();

            DHCPv4ScopeCreateInstruction instruction = new DHCPv4ScopeCreateInstruction
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                ParentId = request.ParentId,
                AddressProperties = GetScopeAddressProperties(request),
                ResolverInformation = GetResolverInformation(request),
                ScopeProperties = GetScopeProperties(request),
            };

            RootScope.AddScope(instruction);
            await SaveRootAndTriggerEvents();

            return id;
        }
    }
}
