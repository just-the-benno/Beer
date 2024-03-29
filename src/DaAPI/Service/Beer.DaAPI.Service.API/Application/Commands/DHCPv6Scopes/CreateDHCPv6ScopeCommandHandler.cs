﻿using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv6Scopes
{
    public class CreateDHCPv6ScopeCommandHandler : ManipulateDHCPv6ScopeCommandHandler, IRequestHandler<CreateDHCPv6ScopeCommand, Guid?>
    {
        private readonly ILogger<CreateDHCPv6ScopeCommandHandler> _logger;

        public CreateDHCPv6ScopeCommandHandler(
            IDHCPv6StorageEngine store, IServiceBus serviceBus, DHCPv6RootScope rootScope,
            ILogger<CreateDHCPv6ScopeCommandHandler> logger) : base(store,serviceBus,rootScope)
        {
            _logger = logger;
        }

        public async Task<Guid?> Handle(CreateDHCPv6ScopeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            Guid id = Guid.NewGuid();

            DHCPv6ScopeCreateInstruction instruction = new DHCPv6ScopeCreateInstruction
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
