using Beer.Identity.Infrastructure.Repositories;
using Beer.Identity.Services;
using IdentityServer4.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Beer.Identity.UnitTests.Services
{
    public class HybridClientStoreTester
    {
        [Fact]
        public async Task FindClientByIdAsync_StaticFound()
        {
            String clientId = "mytestclient";

            List<Client> preconfiguredClients = new()
            {
                new Client { ClientId = clientId },
            };

            HybridClientStore store = new(Mock.Of<IClientRepository>(MockBehavior.Strict), preconfiguredClients);

            Client actual = await store.FindClientByIdAsync(clientId);

            Assert.Equal(preconfiguredClients[0], actual);
        }

        [Fact]
        public async Task FindClientByIdAsync_FoundInRepo()
        {
            String clientId = "mytestclient";

            List<Client> preconfiguredClients = new()
            {
                new Client { ClientId = clientId + "4" },

            };

            BeerClient savedClient = new()
            {
                ClientId = clientId,
                AllowedCorsOrigins = new[] { "https://myapp.com" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = "testclient",
                FrontChannelLogoutUri = "http://mylogout.com/logout",
                HashedPassword = "mypassword".Sha256(),
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback" },
                RedirectUris = new[] { "https://myapp2.com/login-callback" },
                RequirePkce = false,
            };

            Mock<IClientRepository> rempoMock = new(MockBehavior.Strict);
            rempoMock.Setup(x => x.CheckIfClientIdExists(clientId)).ReturnsAsync(true).Verifiable();
            rempoMock.Setup(x => x.GetClientById(clientId)).ReturnsAsync(savedClient).Verifiable();

            HybridClientStore store = new(rempoMock.Object, preconfiguredClients);

            Client actual = await store.FindClientByIdAsync(clientId);

            Assert.NotNull(actual);
            Assert.Single(actual.ClientSecrets);
            Assert.Equal(GrantTypes.Code, actual.AllowedGrantTypes);
            Assert.False(actual.RequireClientSecret);
            Assert.False(actual.RequireConsent);
            Assert.True(actual.AllowOfflineAccess);
            Assert.Equal((new[] { "openid", "profile" }).Union(savedClient.AllowedScopes), actual.AllowedScopes);

            Assert.Equal(savedClient.RequirePkce, actual.RequirePkce);
            Assert.Equal(savedClient.RedirectUris, actual.RedirectUris);
            Assert.Equal(savedClient.AllowedCorsOrigins, actual.AllowedCorsOrigins);
            Assert.Equal(savedClient.FrontChannelLogoutUri, actual.FrontChannelLogoutUri);
            Assert.Equal(savedClient.PostLogoutRedirectUris, actual.PostLogoutRedirectUris);

            rempoMock.Verify();
        }

        [Fact]
        public async Task FindClientByIdAsync_NoStaticAndNotFoundInRepo()
        {
            String clientId = "mytestclient";

            List<Client> preconfiguredClients = new()
            {
                new Client { ClientId = clientId + "4" },
            };

            Mock<IClientRepository> rempoMock = new(MockBehavior.Strict);
            rempoMock.Setup(x => x.CheckIfClientIdExists(clientId)).ReturnsAsync(false).Verifiable();

            HybridClientStore store = new(rempoMock.Object, preconfiguredClients);

            Client actual = await store.FindClientByIdAsync(clientId);

            Assert.Null(actual);

            rempoMock.Verify();
        }

    }
}
