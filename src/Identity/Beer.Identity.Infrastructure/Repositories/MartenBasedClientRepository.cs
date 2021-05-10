using Beer.Identity.Shared.Responses;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Beer.Identity.Shared.Responses.ClientResponses.V1;

namespace Beer.Identity.Infrastructure.Repositories
{
    public class MartenBasedClientRepository : IClientRepository
    {
        private readonly IDocumentStore _store;

        public MartenBasedClientRepository(IDocumentStore store)
        {
            this._store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<Boolean> AddClient(BeerClient client)
        {
            using (var session = _store.LightweightSession())
            {
                session.Store(client);

                await session.SaveChangesAsync();
                return true;
            }
        }

        public async Task<Boolean> CheckIfClientExists(Guid systemId)
        {
            using (var session = _store.LightweightSession())
            {
                var result = await session.Query<BeerClient>().AnyAsync(x => x.Id == systemId);
                return result;
            }
        }

        public async Task<Boolean> CheckIfClientIdExists(String clientId)
        {
            using (var session = _store.LightweightSession())
            {
                var result = await session.Query<BeerClient>().AnyAsync(x => x.ClientId == clientId);
                return result;
            }
        }

        public async Task<Boolean> CheckIfClientIdExists(String clientId, Guid systemId)
        {
            using (var session = _store.LightweightSession())
            {
                var result = await session.Query<BeerClient>().AnyAsync(x => x.ClientId == clientId && x.Id != systemId);
                return result;
            }
        }

        public async Task<Boolean> DeleteClient(Guid systemId)
        {
            using (var session = _store.LightweightSession())
            {
                session.Delete<BeerClient>(systemId);
                await session.SaveChangesAsync();
                return true;
            }
        }

        public async Task<IEnumerable<ClientOverview>> GetAllClientsSortedByName()
        {
            using (var session = _store.QuerySession())
            {
                var result = await session.Query<BeerClient>().OrderBy(x => x.DisplayName)
                    .Select(x => new ClientOverview
                    {
                        AllowedCorsOrigins = x.AllowedCorsOrigins,
                        AllowedScopes = x.AllowedScopes,
                        ClientId = x.ClientId,
                        DisplayName = x.DisplayName,
                        FrontChannelLogoutUri = x.FrontChannelLogoutUri,
                        PostLogoutRedirectUris = x.PostLogoutRedirectUris,
                        RedirectUris = x.RedirectUris,
                        SystemId = x.Id
                    }).ToListAsync();

                return result;
            }
        }

        private async Task<BeerClient> GetClientByPredicate(Expression<Func<BeerClient, bool>> predicate)
        {
            using (var session = _store.QuerySession())
            {
                BeerClient result = await session.Query<BeerClient>().FirstAsync(predicate);
                return result;
            }
        }

        public async Task<BeerClient> GetClientById(Guid systemId) => await GetClientByPredicate(x => x.Id == systemId);
        public async Task<BeerClient> GetClientById(String clientId) => await GetClientByPredicate(x => x.ClientId == clientId);

        public async Task<Boolean> UpdateClient(BeerClient client)
        {
            using (var session = _store.LightweightSession())
            {
                session.Store(client);

                await session.SaveChangesAsync();
                return true;
            }
        }
    }
}
