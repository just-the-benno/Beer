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
    public class UpdateClientCommandHandlerTester
    {
        private static async Task SendRequest(bool repoResult, bool setPassword, bool changeClientId)
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var request = new UpdateClientRequest
            {
                SystemId = clientSystemId,
                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { "https://myapp.com" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                FrontChannelLogoutUri = "http://mylogout.com/logout",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback" },
                RedirectUris = new[] { "https://myapp2.com/login-callback" },
                RequirePkce = false,
            };

            if (setPassword == true)
            {
                request.Password = "mynewchangedpassword";
            }

            BeerClient savedClient = GetSaveBeerClient(random, clientSystemId);


            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientExists(clientSystemId)).ReturnsAsync(true).Verifiable();
            repoMock.Setup(x => x.GetClientById(clientSystemId)).ReturnsAsync(savedClient).Verifiable();

            repoMock.Setup(x => x.UpdateClient(It.Is<BeerClient>(y =>
                y.DisplayName == request.DisplayName
            ))).ReturnsAsync(repoResult).Verifiable();

            if (changeClientId == false)
            {
                request.ClientId = savedClient.ClientId;
            }
            else
            {
                repoMock.Setup(x => x.CheckIfClientIdExists(request.ClientId, request.SystemId)).ReturnsAsync(false).Verifiable();
            }

            var handler = new UpdateClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<UpdateClientCommandHandler>>());

            Boolean actual = await handler.Handle(new UpdateClientCommand(request), CancellationToken.None);

            Assert.Equal(repoResult, actual);

            Assert.Equal(request.ClientId, savedClient.ClientId);
            Assert.Equal(request.AllowedCorsOrigins, savedClient.AllowedCorsOrigins);
            Assert.Equal(request.AllowedScopes, savedClient.AllowedScopes);
            Assert.Equal(request.DisplayName, savedClient.DisplayName);
            Assert.False(savedClient.RequirePkce);
            Assert.Equal(request.FrontChannelLogoutUri, savedClient.FrontChannelLogoutUri);
            if (setPassword == false)
            {
                Assert.Equal("previousPassword".Sha256(), savedClient.HashedPassword);
            }
            else
            {
                Assert.Equal("mynewchangedpassword".Sha256(), savedClient.HashedPassword);
            }

            Assert.Equal(request.PostLogoutRedirectUris, savedClient.PostLogoutRedirectUris);
            Assert.Equal(request.RedirectUris, savedClient.RedirectUris);

            repoMock.Verify();
        }

        private static BeerClient GetSaveBeerClient(Random random, Guid clientSystemId)
        {
            return new BeerClient
            {
                Id = clientSystemId,
                ClientId = random.GetAlphanumericString(),
                AllowedCorsOrigins = new[] { $"https://{random.GetAlphanumericString()}.com" },
                AllowedScopes = new[] { $"test-{random.GetAlphanumericString()}", $"testus-{random.GetAlphanumericString()}" },
                HashedPassword = "previousPassword".Sha256(),
                DisplayName = random.GetAlphanumericString(),
                FrontChannelLogoutUri = $"http://{random.GetAlphanumericString()}.com/logout",
                PostLogoutRedirectUris = new[] { $"https://{random.GetAlphanumericString()}.com/logout-callback" },
                RedirectUris = new[] { $"https://{random.GetAlphanumericString()}.com/login-callback" },
                RequirePkce = true,
            };
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean repoResult)
        {
            await SendRequest(repoResult, false, false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_WithPasswordChange(Boolean repoResult)
        {
            await SendRequest(repoResult, true, false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_WithPasswordChange_WithDisplayChange(Boolean repoResult)
        {
            await SendRequest(repoResult, true, true);
        }

        [Fact]
        public async Task Handle_ParseUrls()
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            BeerClient savedClient = GetSaveBeerClient(random, clientSystemId);

            var request = new UpdateClientRequest
            {
                SystemId = clientSystemId,
                ClientId = savedClient.ClientId,
                AllowedCorsOrigins = new[] { "https://myapp.com/" },
                AllowedScopes = new[] { "test", "testus" },
                DisplayName = random.GetAlphanumericString(),
                FrontChannelLogoutUri = "http://mylogout.com/logout/",
                PostLogoutRedirectUris = new[] { "https://myapp2.com/logout-callback/" },
                RedirectUris = new[] { "https://myapp2.com/login-callback/" },
            };

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientExists(clientSystemId)).ReturnsAsync(true).Verifiable();
            repoMock.Setup(x => x.GetClientById(clientSystemId)).ReturnsAsync(savedClient).Verifiable();

            repoMock.Setup(x => x.UpdateClient(It.Is<BeerClient>(y =>
                 y.DisplayName == request.DisplayName
             ))).ReturnsAsync(true).Verifiable();

            var handler = new UpdateClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<UpdateClientCommandHandler>>());

            Boolean result = await handler.Handle(new UpdateClientCommand(request), CancellationToken.None);

            Assert.True(result);

            Assert.Equal(new[] { "https://myapp.com" }, savedClient.AllowedCorsOrigins);
            Assert.Equal(request.AllowedScopes, savedClient.AllowedScopes);
            Assert.Equal(request.DisplayName, savedClient.DisplayName);
            Assert.Equal("http://mylogout.com/logout", savedClient.FrontChannelLogoutUri);
            Assert.Equal(new[] { "https://myapp2.com/login-callback" }, savedClient.RedirectUris);
            Assert.Equal(new[] { "https://myapp2.com/logout-callback" }, savedClient.PostLogoutRedirectUris);

            repoMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_ClientIdNotFound()
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var request = new UpdateClientRequest
            {
                SystemId = clientSystemId,
            };

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientExists(clientSystemId)).ReturnsAsync(false).Verifiable();

            var handler = new UpdateClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<UpdateClientCommandHandler>>());

            Boolean result = await handler.Handle(new UpdateClientCommand(request), CancellationToken.None);

            Assert.False(result);

            repoMock.Verify();
        }
    }
}
