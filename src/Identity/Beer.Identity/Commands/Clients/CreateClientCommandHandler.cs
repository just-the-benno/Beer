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
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Guid?>
    {
        private readonly IClientRepository _clientRepo;
        private readonly ILogger<CreateClientCommandHandler> _logger;

        public CreateClientCommandHandler(
            IClientRepository clientRepo, ILogger<CreateClientCommandHandler> logger)
        {
            this._clientRepo = clientRepo;
            this._logger = logger;
        }

        public async Task<Guid?> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var content = request.Request;

            if (await _clientRepo.CheckIfClientIdExists(content.ClientId) == true)
            {
                return null;
            }

            BeerClient client = new()
            {
                DisplayName = content.DisplayName,
                ClientId = content.ClientId,
                HashedPassword = content.Password.Sha256(),
                RedirectUris = content.RedirectUris,
                AllowedCorsOrigins = content.AllowedCorsOrigins ?? Array.Empty<String>(),
                FrontChannelLogoutUri = content.FrontChannelLogoutUri ?? String.Empty,
                PostLogoutRedirectUris = content.PostLogoutRedirectUris,
                AllowedScopes = content.AllowedScopes,
                RequirePkce = content.RequirePkce,
            };

            client.ParseUrls();

            var result =  await _clientRepo.AddClient(client);
            return result == true ? client.Id : new Guid?();
        }
    }
}
