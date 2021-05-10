using Beer.Identity.Commands.Clients;
using Beer.Identity.Infrastructure.Repositories;
using Beer.TestHelper;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Beer.Identity.Shared.Requests.ClientRequests.V1;

namespace Beer.Identity.UnitTests.Commands.Clients
{
    public class CreateClientCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean repoResult)
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var request = new CreateClientRequest
            {
                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { "https://myapp.com" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                FrontChannelLogoutUri = "http://mylogout.com/logout",
                Password = "mypassword",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback" },
                RedirectUris = new[] { "https://myapp2.com/login-callback" },
                RequirePkce = true,
            };

            BeerClient clientCreated = null;

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientIdExists(request.ClientId)).ReturnsAsync(false).Verifiable();
            repoMock.Setup(x => x.AddClient(It.Is<BeerClient>(y =>
                y.DisplayName == request.DisplayName
            ))).ReturnsAsync(repoResult).Callback<BeerClient>(x => { clientCreated = x; x.Id = clientSystemId; }).Verifiable();

            var handler = new CreateClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<CreateClientCommandHandler>>());

            Guid? result = await handler.Handle(new CreateClientCommand(request), CancellationToken.None);
            if (repoResult == true)
            {
                Assert.True(result.HasValue);
                Assert.Equal(clientSystemId, result.Value);
            }
            else
            {
                Assert.False(result.HasValue);
            }

            Assert.Equal(request.ClientId, clientCreated.ClientId);
            Assert.Equal(request.AllowedCorsOrigins, clientCreated.AllowedCorsOrigins);
            Assert.Equal(request.AllowedScopes, clientCreated.AllowedScopes);
            Assert.Equal(request.DisplayName, clientCreated.DisplayName);
            Assert.Equal(request.FrontChannelLogoutUri, clientCreated.FrontChannelLogoutUri);
            Assert.Equal("mypassword".Sha256(), clientCreated.HashedPassword);
            Assert.Equal(request.PostLogoutRedirectUris, clientCreated.PostLogoutRedirectUris);
            Assert.Equal(request.RedirectUris, clientCreated.RedirectUris);
            Assert.True(clientCreated.RequirePkce);

            repoMock.Verify();
        }

        [Fact]
        public async Task Handle_ParseUrls()
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var request = new CreateClientRequest
            {
                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { "https://myapp.com/" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                FrontChannelLogoutUri = "http://mylogout.com/logout/",
                Password = "mypassword",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                RedirectUris = new[] { "https://myapp2.com/login-callback/" },
            };

            BeerClient clientCreated = null;

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientIdExists(request.ClientId)).ReturnsAsync(false).Verifiable();
            repoMock.Setup(x => x.AddClient(It.Is<BeerClient>(y =>
                y.DisplayName == request.DisplayName
            ))).ReturnsAsync(true).Callback<BeerClient>(x => { clientCreated = x; x.Id = clientSystemId; }).Verifiable();

            var handler = new CreateClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<CreateClientCommandHandler>>());

            Guid? result = await handler.Handle(new CreateClientCommand(request), CancellationToken.None);

            Assert.True(result.HasValue);
            Assert.Equal(clientSystemId, result.Value);

            Assert.Equal(new[] { "https://myapp.com" }, clientCreated.AllowedCorsOrigins);
            Assert.Equal(request.AllowedScopes, clientCreated.AllowedScopes);
            Assert.Equal(request.DisplayName, clientCreated.DisplayName);
            Assert.Equal("http://mylogout.com/logout", clientCreated.FrontChannelLogoutUri);
            Assert.Equal(new[] { "https://myapp2.com/login-callback" }, clientCreated.RedirectUris);
            Assert.Equal(new[] { "https://myapp2.com/logout-callback" }, clientCreated.PostLogoutRedirectUris);

            repoMock.Verify();
        }

        [Fact]
        public async Task Handle_DefaultValues()
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var request = new CreateClientRequest
            {
                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = null,
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                FrontChannelLogoutUri = null,
                Password = "mypassword",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback" },
                RedirectUris = new[] { "https://myapp2.com/login-callback" },
            };

            BeerClient clientCreated = null;

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientIdExists(request.ClientId)).ReturnsAsync(false).Verifiable();
            repoMock.Setup(x => x.AddClient(It.Is<BeerClient>(y =>
                y.DisplayName == request.DisplayName
            ))).ReturnsAsync(true).Callback<BeerClient>(x => { clientCreated = x; x.Id = clientSystemId; }).Verifiable();

            var handler = new CreateClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<CreateClientCommandHandler>>());

            Guid? result = await handler.Handle(new CreateClientCommand(request), CancellationToken.None);

            Assert.True(result.HasValue);
            Assert.Equal(clientSystemId, result.Value);

            Assert.NotNull(clientCreated.AllowedCorsOrigins);
            Assert.Empty(clientCreated.AllowedCorsOrigins);

            Assert.Equal(String.Empty, clientCreated.FrontChannelLogoutUri);

            Assert.Equal(request.AllowedScopes, clientCreated.AllowedScopes);
            Assert.Equal(request.DisplayName, clientCreated.DisplayName);
            Assert.Equal(new[] { "https://myapp2.com/login-callback" }, clientCreated.RedirectUris);
            Assert.Equal(new[] { "https://myapp2.com/logout-callback" }, clientCreated.PostLogoutRedirectUris);

            repoMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_ClientIdExsits()
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var request = new CreateClientRequest
            {
                ClientId = random.GetAlphanumericString(),
            };

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientIdExists(request.ClientId)).ReturnsAsync(true).Verifiable();

            var handler = new CreateClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<CreateClientCommandHandler>>());

            Guid? result = await handler.Handle(new CreateClientCommand(request), CancellationToken.None);

            Assert.False(result.HasValue);

            repoMock.Verify();
        }
    }
}
