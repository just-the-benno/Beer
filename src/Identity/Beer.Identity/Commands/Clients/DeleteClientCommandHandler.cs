using Beer.Identity.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.Identity.Commands.Clients
{
    public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Boolean>
    {
        private readonly IClientRepository _clientRepo;
        private readonly ILogger<DeleteClientCommandHandler> _logger;

        public DeleteClientCommandHandler(
            IClientRepository clientRepo, ILogger<DeleteClientCommandHandler> logger)
        {
            this._clientRepo = clientRepo;
            this._logger = logger;
        }

        public async Task<Boolean> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            if (await _clientRepo.CheckIfClientExists(request.SystemId) == false)
            {
                return false;
            }

            Boolean result = await _clientRepo.DeleteClient(request.SystemId);
            return result;
        }
    }
}
