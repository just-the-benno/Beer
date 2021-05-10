using Beer.Identity.Infrastructure.Data;
using Beer.Identity.Services;
using Beer.Identity.Utilities;
using Beer.TestHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using static Beer.Identity.Shared.Requests.LocalUserRequests.V1;
using static Beer.Identity.Shared.Responses.LocalUsersResponses.V1;

namespace Beer.Identity.IntegrationTests.ApiControllers
{
    public class LocalUserControllerTester : BeerIdentityControllerTeserBase,
        IClassFixture<CustomWebApplicationFactory<FakeStartup>>
    {

        public LocalUserControllerTester(CustomWebApplicationFactory<FakeStartup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Get_IndexPageNotFound()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                var client = GetClientWithAnonymousUser(builder);
                var response = await client.GetAsync("/");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task CheckIfItIsNotInitilzied()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                var client = GetClientWithAnonymousUser(builder);

                var response = await client.GetAsync("/api/LocalUsers/Initilized");

                Boolean responseValue = await GetResponseContent<bool>(response);
                Assert.False(responseValue);
            });
        }

        [Fact]
        public async Task CheckIfItIsNotInitilzied_UserExists()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
           {
               Random random = new Random();

               context.Users.Add(new BeerUser { Id = Guid.NewGuid().ToString(), UserName = "Blub", NormalizedUserName = "blub", PasswordHash = random.GetAlphanumericString(), NormalizedEmail = "blub@blub.com", Email = "blub@blub.com" });
               await context.SaveChangesAsync();

               var client = GetClientWithAnonymousUserAndSpecifiedBeerDb(builder);
               var response = await client.GetAsync("/api/LocalUsers/Initilized");

               Boolean responseValue = await GetResponseContent<bool>(response);
               Assert.True(responseValue);
           });
        }

        [Fact]
        public async Task GetAllUsers()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new Random();

                List<LocalUserOverview> expectedResult = new List<LocalUserOverview>
                {
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Yankee", DisplayName = "my Yankee"  },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Bravo" , DisplayName = "your Bravo" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Hotel" , DisplayName = "in a hotel" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Romeo" , DisplayName = "Juliet"},
                };

                foreach (var item in expectedResult)
                {
                    context.Users.Add(new BeerUser { Id = item.Id, UserName = item.LoginName, NormalizedUserName = item.LoginName.ToUpper(), PasswordHash = random.GetAlphanumericString(), NormalizedEmail = $"{item.LoginName.ToUpper()}@blub.com", Email = $"{item.LoginName}@blub.com" });
                    context.UserClaims.AddRange(new IdentityUserClaim<string>[]
                    {
                        new IdentityUserClaim<string>{ UserId = item.Id, ClaimType = CustomClaimTypes.DisplayClaimsType, ClaimValue = item.DisplayName },
                        new IdentityUserClaim<string>{ UserId = item.Id, ClaimType = CustomClaimTypes.ProfilePictureUrl, ClaimValue = $"https://mypicutre/{random.NextGuid()}.png" },
                    });
                }

                await context.SaveChangesAsync();

                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, random.NextGuid(), AuthenticationDefaults.BeerUserListScope);
                var response = await client.GetAsync("/api/LocalUsers");

                var responseValue = await GetResponseContent<IEnumerable<LocalUserOverview>>(response);

                Assert.Equal(expectedResult.OrderBy(x => x.LoginName), responseValue, new LocalUserOverviewEqualityComparer());
            });
        }

        [Fact]
        public async Task GetAvailableAvatars()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new();

                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, random.NextGuid(), AuthenticationDefaults.BeerUserListScope);
                var response = await client.GetAsync("/api/LocalUsers/Avatars");

                var responseValue = await GetResponseContent<IEnumerable<String>>(response);

                String path = "http://localhost/img/pp";
                IEnumerable<String> expectedResult = new String[]
                {
                    $"{path}/animal-avatar-1.png",
                    $"{path}/animal-avatar-2.png",
                    $"{path}/animal-avatar-3.png",
                    $"{path}/animal-avatar-4.png",
                    $"{path}/animal-avatar-5.png",
                    $"{path}/animal-avatar-6.png",
                    $"{path}/animal-avatar-7.png",
                    $"{path}/animal-avatar-8.png",
                    $"{path}/animal-avatar-9.png",

                    $"{path}/other-animal-avatar-1.png",
                    $"{path}/other-animal-avatar-2.png",
                    $"{path}/other-animal-avatar-3.png",
                    $"{path}/other-animal-avatar-4.png",
                    $"{path}/other-animal-avatar-5.png",
                    $"{path}/other-animal-avatar-6.png",

                    $"{path}/dogface-1.png",
                    $"{path}/dogface-2.png",
                    $"{path}/dogface-3.png",
                    $"{path}/dogface-4.png",
                    $"{path}/dogface-5.png",
                    $"{path}/dogface-6.png",
                    $"{path}/dogface-7.png",
                    $"{path}/dogface-8.png",
                    $"{path}/dogface-9.png",
                };

                Assert.Equal(expectedResult.OrderBy(x => x), responseValue.OrderBy(x => x));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckIfUserNameExists(Boolean shouldExsits)
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new();
                String username = "testUseR244";

                if (shouldExsits == true)
                {
                    String userId = random.NextGuid().ToString();
                    context.Users.Add(new BeerUser { Id = userId, UserName = username, NormalizedUserName = username.ToUpper(), PasswordHash = random.GetAlphanumericString(), NormalizedEmail = $"{username.ToUpper()}@blub.com", Email = $"{username}@blub.com" });
                    context.UserClaims.AddRange(new IdentityUserClaim<string>[]
                    {
                        new IdentityUserClaim<string>{ UserId = userId, ClaimType = CustomClaimTypes.DisplayClaimsType, ClaimValue = "my display value" },
                        new IdentityUserClaim<string>{ UserId = userId, ClaimType = CustomClaimTypes.ProfilePictureUrl, ClaimValue = $"https://mypicutre/{random.NextGuid()}.png" },
                    });

                    await context.SaveChangesAsync();
                }


                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, random.NextGuid(), AuthenticationDefaults.BeerUserListScope);

                String[] nameVariants = new[] { username, username.ToLower(), username.ToUpper() };
                foreach (var item in nameVariants)
                {
                    var response = await client.GetAsync($"/api/LocalUsers/Exists/{item}");

                    var normalResponseValue = await GetResponseContent<Boolean>(response);
                    Assert.Equal(shouldExsits, normalResponseValue);
                }
            });
        }

        [Fact]
        public async Task ResetUserPassword()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new Random();

                Guid userId = random.NextGuid();
                String oldPasswordHash = random.GetAlphanumericString();
                String newPassword = "P@ssw0rd11!!2_a";

                context.Users.Add(new BeerUser { Id = userId.ToString(), UserName = "Alice", NormalizedUserName = "alice", PasswordHash = oldPasswordHash, NormalizedEmail = "blub@blub.com", Email = "blub@blub.com" });

                await context.SaveChangesAsync();

                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, random.NextGuid(), AuthenticationDefaults.BeerUserResetPasswordScope);
                var response = await client.PutAsync($"/api/LocalUsers/ChangePassword/{userId}", new ResetPasswordRequest
                {
                    Password = newPassword
                });

                await IsEmptyResult(response);
            });
        }

        [Fact]
        public async Task DeleteUser_Success()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new Random();

                List<LocalUserOverview> expectedResult = new List<LocalUserOverview>
                {
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Yankee" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Bravo" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Hotel" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Romeo" },
                };

                foreach (var item in expectedResult)
                {
                    context.Users.Add(new BeerUser { Id = item.Id, UserName = item.LoginName, NormalizedUserName = item.LoginName.ToLower(), PasswordHash = random.GetAlphanumericString(), NormalizedEmail = $"{item.LoginName.ToLower()}@blub.com", Email = $"{item.LoginName}@blub.com" });
                }

                await context.SaveChangesAsync();

                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, random.NextGuid(), AuthenticationDefaults.BeerUserDeleteScope);

                while (expectedResult.Count > 1)
                {
                    LocalUserOverview user = expectedResult.GetRandomElement(random);

                    var response = await client.DeleteAsync($"/api/LocalUsers/{user.Id}");

                    await IsEmptyResult(response);

                    expectedResult.Remove(user);
                }
            });
        }

        [Fact]
        public async Task DeleteUser_CantDeleteYourself()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new Random();

                List<LocalUserOverview> expectedResult = new List<LocalUserOverview>
                {
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Yankee" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Bravo" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Hotel" },
                    new LocalUserOverview { Id = random.NextGuid().ToString(), LoginName = "Romeo" },
                };

                foreach (var item in expectedResult)
                {
                    context.Users.Add(new BeerUser { Id = item.Id, UserName = item.LoginName, NormalizedUserName = item.LoginName.ToLower(), PasswordHash = random.GetAlphanumericString(), NormalizedEmail = $"{item.LoginName.ToLower()}@blub.com", Email = $"{item.LoginName}@blub.com" });
                }

                await context.SaveChangesAsync();
                LocalUserOverview user = expectedResult.GetRandomElement(random);

                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, Guid.Parse(user.Id), AuthenticationDefaults.BeerUserDeleteScope);

                var response = await client.DeleteAsync($"/api/LocalUsers/{user.Id}");

                IsBadRequest(response);
            });
        }

        [Fact]
        public async Task DeleteUser_LastUserCantBeDeleted()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new Random();

                Guid id = random.NextGuid();

                context.Users.Add(new BeerUser { Id = id.ToString(), UserName = "Alice", NormalizedUserName = "alice", PasswordHash = random.GetAlphanumericString(), NormalizedEmail = $"Blub@blub.com", Email = $"blub@blub.com" });

                await context.SaveChangesAsync();

                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, random.NextGuid(), AuthenticationDefaults.BeerUserDeleteScope);

                var response = await client.DeleteAsync($"/api/LocalUsers/{id}");

                IsBadRequest(response);
            });
        }

        [Fact]
        public async Task CreateUser()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                Random random = new Random();

                String newPassword = "P@ssw0rd11!!2_a";
                String username = "Alice";
                String displayName = "Alice in the wonderland";
                String imgUrl = "/img/pp/animal-avatar-7.png";

                var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDb(builder, random.NextGuid(), AuthenticationDefaults.BeerUserCreateScope);
                var response = await client.PostAsync($"/api/LocalUsers/", new CreateUserRequest
                {
                    Username = username,
                    Password = newPassword,
                    DisplayName = displayName,
                    ProfilePictureUrl = imgUrl,
                });

                String id = await IsObjectResult<String>(response);

                var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
                Assert.NotNull(user);

                Assert.Equal(username, user.UserName);

                var claims = await context.UserClaims.Where(x => x.UserId == user.Id).ToListAsync();

                Assert.Equal(displayName, claims.First(x => x.ClaimType == "displayName").ClaimValue);
                Assert.False(String.IsNullOrEmpty(claims.First(x => x.ClaimType == "picture").ClaimValue));
            });
        }
    }
}
