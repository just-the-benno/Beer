using Beer.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Beer.Identity.Responses.LocalUsersResponses.V1;

namespace Beer.Identity.Services
{
    public static class CustomClaimTypes
    {
        public static String DisplayClaimsType => "displayName";
        public static String ProfilePictureUrl => "picture";
    }

    public class LocalUserManagerService : ILocalUserService
    {
        private readonly UserManager<BeerUser> _userManager;

        public LocalUserManagerService(UserManager<BeerUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Guid?> CreateUser(String username, String password, String displayName, String profilePictureUrl)
        {
            Guid userId = Guid.NewGuid();

            var user = new BeerUser { UserName = username, Id = userId.ToString() };
            //DisplayName = displayName, ProfilePictureUrl = profilePictureUrl };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded == true)
            {
                await _userManager.AddClaimsAsync(user,new Claim[]
                {
                    new Claim(CustomClaimTypes.DisplayClaimsType,displayName),
                    new Claim(CustomClaimTypes.ProfilePictureUrl,profilePictureUrl),
                });;

                return userId;
            }
            else
            {
                return null;
            }
        }

        public async Task<Int32> GetUserAmount() => await _userManager.Users.CountAsync();

        public async Task<IEnumerable<LocalUserOverview>> GetAllUsersSortedByName()
        {
            var result = await _userManager.Users.OrderBy(x => x.NormalizedUserName).Select(x =>  new LocalUserOverview
            {
                Id = x.Id,
                LoginName = x.UserName,
            }).ToListAsync();

            foreach (var item in result)
            {
                var claims = await _userManager.GetClaimsAsync(new BeerUser { Id = item.Id });
                item.DisplayName = claims.FirstOrDefault(x => x.Type == CustomClaimTypes.DisplayClaimsType)?.Value;
            }

            return result;
        }

        public async Task<Boolean> DeleteUser(String userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded;
        }

        public async Task<Boolean> CheckIfUserExists(String userId)
            => await _userManager.Users.CountAsync(x => x.Id == userId) > 0;

        public async Task<Boolean> ResetPassword(String userId, String password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { return false; }

            foreach (var item in _userManager.PasswordValidators)
            {
                var validationResult = await item.ValidateAsync(_userManager, null, password);
                if (validationResult.Succeeded == false)
                {
                    return false;
                }
            }

            IdentityResult removeResult = await _userManager.RemovePasswordAsync(user);
            if (removeResult.Succeeded == false) { return false; }

            IdentityResult result = await _userManager.AddPasswordAsync(user, password);
            return result.Succeeded;
        }

        public async Task<Boolean> CheckIfUsernameExists(string username) => (await _userManager.FindByNameAsync(username)) != null;
    }
}
