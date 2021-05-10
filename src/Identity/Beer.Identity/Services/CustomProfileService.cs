using Beer.Identity.Infrastructure.Data;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Beer.Identity.Services
{
    public class CustomProfileService : ProfileService<BeerUser>
    {
        private readonly IHttpContextAccessor _httpRequestAccessor;

        public CustomProfileService(IHttpContextAccessor httpRequestAccessor, UserManager<BeerUser> userManager,
            IUserClaimsPrincipalFactory<BeerUser> claimsFactory) : base(userManager, claimsFactory)
        {
            this._httpRequestAccessor = httpRequestAccessor;
        }

        protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, BeerUser user)
        {
            var principal = await GetUserClaimsAsync(user);

            if (context.Caller == IdentityServerConstants.ProfileDataCallers.UserInfoEndpoint)
            {
                var claims = await UserManager.GetClaimsAsync(user);
                var identity = principal.Identities.First();

                if (context.RequestedResources.ParsedScopes.FirstOrDefault(x => x.ParsedName == "profile") != null)
                {
                    SwapClaims(identity, CustomClaimTypes.DisplayClaimsType, JwtClaimTypes.PreferredUserName, true);

                    var pictureClaim = identity.FindFirst(JwtClaimTypes.Picture);
                    if(pictureClaim != null)
                    {
                        String value = pictureClaim.Value;
                        var request = _httpRequestAccessor.HttpContext.Request;
                        String requestUri = $"{request.Scheme}://{request.Host}{value}";

                        identity.RemoveClaim(pictureClaim);
                        identity.AddClaim(new Claim(JwtClaimTypes.Picture, requestUri));
                    }
                }
                //temporary fix to add an email address claim if required
                if(context.RequestedResources.ParsedScopes.FirstOrDefault(x => x.ParsedName == "email") != null)
                {
                    identity.AddClaim(new Claim(JwtClaimTypes.Email, $"{identity.FindFirst(JwtClaimTypes.Name).Value}@beerusers.com"));
                }
            }

            context.AddRequestedClaims(principal.Claims);
        }

        private static void SwapClaims(ClaimsIdentity identity, String existingType, String targetType, Boolean removeExisting)
        {
            var existingSourceClaim = identity.FindFirst(existingType);
            if (existingSourceClaim != null)
            {
                var targetClaim = identity.FindFirst(targetType);
                if (targetClaim != null)
                {
                    identity.RemoveClaim(targetClaim);
                }

                identity.AddClaim(new Claim(targetType, existingSourceClaim.Value));

                if(removeExisting == true)
                {
                    identity.RemoveClaim(existingSourceClaim);
                }
            }
        }
    }
}
