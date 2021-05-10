using Beer.ControlCenter.BlazorApp.Pages.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.ControlCenter.BlazorApp.Services.Requests.BeerClientRequests.V1;
using static Beer.ControlCenter.BlazorApp.Services.Responses.BeerClientResponses.V1;

namespace Beer.ControlCenter.BlazorApp.Services
{
    public interface IOpenIdService
    {
        Task<IEnumerable<ClientOverview>> GetAllOpenIdClients();
        Task<Boolean> CreateClient(CreateBeerOpenIdClientRequest request);
        Task<Boolean> UpdateClient(UpdateBeerOpenIdClientRequest request);
        Task<Boolean> DeleteClient(Guid systemId);
        Task<OpenIdEndpoints> GetOpenIdConfiguration();
    }
}
