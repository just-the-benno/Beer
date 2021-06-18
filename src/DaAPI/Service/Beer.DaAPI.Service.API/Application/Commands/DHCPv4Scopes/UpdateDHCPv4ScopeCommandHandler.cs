using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv4Scopes
{
    public class UpdateDHCPv4ScopeCommandHandler : ManipulateDHCPv4ScopeCommandHandler, IRequestHandler<UpdateDHCPv4ScopeCommand, Boolean>
    {
        private readonly ILogger<UpdateDHCPv4ScopeCommandHandler> _logger;

        public UpdateDHCPv4ScopeCommandHandler(
            IDHCPv4StorageEngine store,
            IServiceBus serviceBus,
            DHCPv4RootScope rootScope,
            ILogger<UpdateDHCPv4ScopeCommandHandler> logger) : base(store, serviceBus, rootScope)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Boolean> Handle(UpdateDHCPv4ScopeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var scope = RootScope.GetScopeById(request.ScopeId);
            if (scope == DHCPv4Scope.NotFound)
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
