using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv6Scopes
{
    public class UpdateDHCPv6ScopeCommandHandler : ManipulateDHCPv6ScopeCommandHandler, IRequestHandler<UpdateDHCPv6ScopeCommand, Boolean>
    {
        private readonly ILogger<UpdateDHCPv6ScopeCommandHandler> _logger;

        public UpdateDHCPv6ScopeCommandHandler(
            IDHCPv6StorageEngine store,
            IServiceBus serviceBus,
            DHCPv6RootScope rootScope,
            ILogger<UpdateDHCPv6ScopeCommandHandler> logger) : base(store,serviceBus,rootScope)
        {
            _logger = logger;
        }

        public async Task<Boolean> Handle(UpdateDHCPv6ScopeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var scope = RootScope.GetScopeById(request.ScopeId);
            if (scope == DHCPv6Scope.NotFound)
            {
                return false;
            }

            Guid? parentId = scope.HasParentScope() == false ? new Guid?() : scope.ParentScope.Id;
            var properties = GetScopeProperties(request);
            var addressProperties = GetScopeAddressProperties(request);

            if (request.Name != scope.Name)
            {
                RootScope.UpdateScopeName(request.ScopeId, ScopeName.FromString(request.Name));
            }
            if (request.Description != scope.Description)
            {
                RootScope.UpdateScopeDescription(request.ScopeId, ScopeDescription.FromString(request.Description));
            }
            if (request.ParentId != parentId)
            {
                RootScope.UpdateParent(request.ScopeId, request.ParentId);
            }

            RootScope.UpdateScopeResolver(request.ScopeId, GetResolverInformation(request));

            if (addressProperties != scope.AddressRelatedProperties)
            {
                RootScope.UpdateAddressProperties(request.ScopeId, addressProperties);
            }

            if (properties != scope.Properties)
            {
                RootScope.UpdateScopeProperties(request.ScopeId, properties);
            }

            Boolean result = await SaveRootAndTriggerEvents();
            return result;
        }
    }
}
