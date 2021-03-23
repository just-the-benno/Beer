using Beer.ControlCenter.BlazorApp.Pages.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.ControlCenter.BlazorApp.Services.Requests.BeerUserRequests.V1;
using static Beer.ControlCenter.BlazorApp.Services.Responses.BeerUserResponses.V1;

namespace Beer.ControlCenter.BlazorApp.Services
{
    public interface IBeerUserService
    {
        Task<IEnumerable<BeerUserOverview>> GetLocalUsers();
        Task<Boolean> ResetPassword(String userId, String password);
        Task<Boolean> CreateUser(CreateBeerUserRequest request);
        Task<Boolean> DeleteUser(String userId);
        Task<Boolean> CheckIfUsernameExists(String name);
        Task<IEnumerable<String>> GetAvailableAvatars();
    }
}
