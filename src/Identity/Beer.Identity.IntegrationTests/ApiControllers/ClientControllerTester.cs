using Beer.Identity.Infrastructure.Data;
using Beer.Identity.Infrastructure.Repositories;
using Beer.Identity.Services;
using Beer.Identity.Utilities;
using Beer.TestHelper;
using IdentityServer4.Models;
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
using static Beer.Identity.Shared.Requests.ClientRequests.V1;
using static Beer.Identity.Shared.Requests.LocalUserRequests.V1;
using static Beer.Identity.Shared.Responses.LocalUsersResponses.V1;

namespace Beer.Identity.IntegrationTests.ApiControllers
{
    public class ClientControllerTester : BeerIdentityControllerTeserBase,
        IClassFixture<CustomWebApplicationFactory<FakeStartup>>
    {

        public ClientControllerTester(CustomWebApplicationFactory<FakeStartup> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData("/api/Clients")]
        public async Task GetAllClients_NotAuthenticate(String url)
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
            {
                var client = GetClientWithAnonymousUser(builder);
                var response = await client.GetAsync(url);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateClientById()
        {
            await ExecuteBeerIdentityContextAwareTest(async (builder, context) =>
           {
               await ExecuteMartenBasedClientRepositoryAwareTest(async (store, repo) =>
              {

                  Random random = new Random();

                  String password = random.GetAlphanumericString();

                  var request = new CreateClientRequest
                  {
                      ClientId = random.GetAlphanumericString(),
                      AllowedCorsOrigins = new[] { "https://myapp.com" },
                      AllowedScopes = new[] { "test", "testus" },
                      DisplayName = random.GetAlphanumericString(),
                      FrontChannelLogoutUri = "http://mylogout.com/logout",
                      Password = password,
                      PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback" },
                      RedirectUris = new[] { "https://myapp2.com/login-callback" },
                  };

                  var client = GetClientWithAuthenticatedUserAndSpecifiedBeerDbAndRepo(builder, repo, random.NextGuid(), AuthenticationDefaults.BeerClientModifyScope);
                  var response = await client.PostAsync($"/api/Clients/", request);

                  Guid responseValue = await GetResponseContent<Guid>(response);

                  using (var session = store.QuerySession())
                  {
                      var dbItem = session.Query<BeerClient>().FirstOrDefault(x => x.Id == responseValue);
                      Assert.NotNull(dbItem);

                      Assert.Equal(request.ClientId, dbItem.ClientId);
                      Assert.Equal(request.Password.Sha256(), dbItem.HashedPassword);
                  }
              });
           });
        }
    }
}
