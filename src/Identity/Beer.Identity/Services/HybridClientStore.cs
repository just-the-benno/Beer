using Beer.Identity.Infrastructure.Repositories;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Services
{
    public class HybridClientStore : IClientStore
    {
        private readonly IClientRepository _clientRepository;
        private readonly IEnumerable<Client> _staticClients;

        public HybridClientStore(IClientRepository clientRepository, IEnumerable<Client> staticClients)
        {
            this._clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _staticClients = staticClients ?? Array.Empty<Client>();
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var staticClient = _staticClients.FirstOrDefault(x => x.ClientId == clientId);
            if (staticClient != null)
            {
                return staticClient;
            }

            if (await _clientRepository.CheckIfClientIdExists(clientId) == false)
            {
                return null;
            }

            var savedClient = await _clientRepository.GetClientById(clientId);

            return new Client
            {
                ClientId = savedClient.ClientId,
                ClientSecrets = { new Secret(savedClient.HashedPassword) },

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = savedClient.RequirePkce,
                RequireClientSecret = false,
                RequireConsent = false,
                AllowOfflineAccess = true,

                AllowedScopes = (new[] { "openid", "profile" }).Union(savedClient.AllowedScopes).ToList(),

                RedirectUris = savedClient.RedirectUris.ToList(),
                AllowedCorsOrigins = savedClient.AllowedCorsOrigins.ToList(),
                FrontChannelLogoutUri = savedClient.FrontChannelLogoutUri,
                PostLogoutRedirectUris = savedClient.PostLogoutRedirectUris.ToList(),
            };
        }
    }
}
