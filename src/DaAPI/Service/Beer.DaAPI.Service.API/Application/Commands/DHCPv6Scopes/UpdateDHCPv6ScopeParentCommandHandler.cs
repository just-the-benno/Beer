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
    public class UpdateDHCPv6ScopeParentCommandHandler : IRequestHandler<UpdateDHCPv6ScopeParentCommand, Boolean>
    {
        private readonly IDHCPv6StorageEngine _store;
        private readonly DHCPv6RootScope _rootScope;
        private readonly ILogger<UpdateDHCPv6ScopeParentCommandHandler> _logger;

        public UpdateDHCPv6ScopeParentCommandHandler(
            IDHCPv6StorageEngine store,
            DHCPv6RootScope rootScope,
            ILogger<UpdateDHCPv6ScopeParentCommandHandler> logger)
        {
            _store = store;
            _rootScope = rootScope;
            _logger = logger;
        }

        public async Task<Boolean> Handle(UpdateDHCPv6ScopeParentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var scope = _rootScope.GetScopeById(request.ScopeId);
            if (scope == DHCPv6Scope.NotFound)
            {
                return false;
            }

            if (request.ParentScopeId.HasValue == true)
            {
                if (_rootScope.GetScopeById(request.ParentScopeId.Value) == DHCPv6Scope.NotFound)
                {
                    return false;
                }
            }

            Boolean moveResult = _rootScope.UpdateParent(request.ScopeId, request.ParentScopeId);
            if (moveResult == true)
            {
                Boolean result = await _store.Save(_rootScope);
                return result;
            }

            return false;
        }
    }
}
