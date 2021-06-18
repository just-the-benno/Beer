using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv4Scopes
{
    public class DeleteDHCPv4ScopeCommandHandler : DHCPv4ScopeTriggerAwareCommandHandler, IRequestHandler<DeleteDHCPv4ScopeCommand, Boolean>
    {
        private readonly ILogger<DeleteDHCPv4ScopeCommandHandler> logger;

        public DeleteDHCPv4ScopeCommandHandler(
            IDHCPv4StorageEngine store,
            IServiceBus serviceBus,
            DHCPv4RootScope rootScope,
            ILogger<DeleteDHCPv4ScopeCommandHandler> logger
            ) : base(store, serviceBus,rootScope)
        {
            this.logger = logger;
        }

        public async Task<Boolean> Handle(DeleteDHCPv4ScopeCommand request, CancellationToken cancellationToken)
        {
            if (RootScope.GetScopeById(request.ScopeId) == DHCPv4Scope.NotFound)
            {
                logger.LogInformation("unable to delete the scope {scopeId}. Scope not found");
                return false;
            }

            RootScope.DeleteScope(request.ScopeId, request.IncludeChildren);

            Boolean result = await SaveRootAndTriggerEvents();
            if (result == false)
            {
                logger.LogError("unable to delete the scope {scopeId}. Saving changes failed", request.ScopeId);
            }
            return result;
        }
    }
}
