using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv4Scopes
{
    public class UpdateDHCPv4ScopeParentCommandHandler : IRequestHandler<UpdateDHCPv4ScopeParentCommand, Boolean>
    {
        private readonly IDHCPv4StorageEngine _store;
        private readonly DHCPv4RootScope _rootScope;
        private readonly ILogger<UpdateDHCPv4ScopeParentCommandHandler> _logger;

        public UpdateDHCPv4ScopeParentCommandHandler(
            IDHCPv4StorageEngine store,
            DHCPv4RootScope rootScope,
            ILogger<UpdateDHCPv4ScopeParentCommandHandler> logger)
        {
            _store = store;
            _rootScope = rootScope;
            _logger = logger;
        }

        public async Task<Boolean> Handle(UpdateDHCPv4ScopeParentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var scope = _rootScope.GetScopeById(request.ScopeId);
            if (scope == DHCPv4Scope.NotFound)
            {
                return false;
            }

            if (request.ParentScopeId.HasValue == true)
            {
                if (_rootScope.GetScopeById(request.ParentScopeId.Value) == DHCPv4Scope.NotFound)
                {
                    return false;
                }
            }

            Boolean moveResult = _rootScope.UpdateParent(request.ScopeId, request.ParentScopeId);
            if(moveResult == true)
            {
                Boolean result = await _store.Save(_rootScope);
                return result;
            }

            return false;
        }
    }
}
