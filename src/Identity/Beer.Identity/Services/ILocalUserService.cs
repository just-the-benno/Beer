using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.Identity.Shared.Responses.LocalUsersResponses.V1;

namespace Beer.Identity.Services
{
    public interface ILocalUserService
    {
        Task<Int32> GetUserAmount();
        Task<Guid?> CreateUser(String username, String password, String displayName, String profilePictureUrl);
        Task<IEnumerable<LocalUserOverview>> GetAllUsersSortedByName();
        Task<Boolean> DeleteUser(String userId);
        Task<Boolean> CheckIfUserExists(String userId);
        Task<Boolean> ResetPassword(String userId, String password);
        Task<Boolean> CheckIfUsernameExists(String username);
    }
}
