using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.Identity.Shared.Responses.ClientResponses.V1;

namespace Beer.Identity.Infrastructure.Repositories
{
    public interface IClientRepository
    {
        Task<IEnumerable<ClientOverview>> GetAllClientsSortedByName();
        Task<Boolean> CheckIfClientExists(Guid systemId);
        Task<Boolean> DeleteClient(Guid systemId);
        Task<Boolean> AddClient(BeerClient client);
        Task<Boolean> CheckIfClientIdExists(String clientId);
        Task<Boolean> CheckIfClientIdExists(String clientId, Guid systemId);
        Task<BeerClient> GetClientById(Guid systemId);
        Task<BeerClient> GetClientById(String clientId);

        Task<Boolean> UpdateClient(BeerClient client);
    }
}
