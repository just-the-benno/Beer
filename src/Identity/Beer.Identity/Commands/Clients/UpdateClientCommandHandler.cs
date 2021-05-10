using Beer.Identity.Infrastructure.Repositories;
using IdentityServer4.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.Identity.Commands.Clients
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Boolean>
    {
        private readonly IClientRepository _clientRepo;
        private readonly ILogger<UpdateClientCommandHandler> _logger;

        public UpdateClientCommandHandler(
            IClientRepository clientRepo, ILogger<UpdateClientCommandHandler> logger)
        {
            this._clientRepo = clientRepo;
            this._logger = logger;
        }

        public async Task<Boolean> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var content = request.Request;

            if (await _clientRepo.CheckIfClientExists(content.SystemId) == false)
            {
                return false;
            }

            BeerClient client = await _clientRepo.GetClientById(content.SystemId);
            if (String.IsNullOrEmpty(content.Password) == false)
            {
                client.HashedPassword = content.Password.Sha256();
            }

            if(client.ClientId != content.ClientId)
            {
                if (await _clientRepo.CheckIfClientIdExists(content.ClientId, content.SystemId) == true)
                {
                    return false;
                }
            }

            client.DisplayName = content.DisplayName;
            client.ClientId = content.ClientId;
            client.RedirectUris = content.RedirectUris;
            client.AllowedCorsOrigins = content.AllowedCorsOrigins ?? Array.Empty<String>();
            client.FrontChannelLogoutUri = content.FrontChannelLogoutUri ?? String.Empty;
            client.PostLogoutRedirectUris = content.PostLogoutRedirectUris;
            client.AllowedScopes = content.AllowedScopes;
            client.RequirePkce = content.RequirePkce;

            client.ParseUrls();

            var result = await _clientRepo.UpdateClient(client);
            return result;
        }
    }
}
