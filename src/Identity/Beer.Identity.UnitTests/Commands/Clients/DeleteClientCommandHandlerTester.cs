using Beer.Identity.Commands.Clients;
using Beer.Identity.Infrastructure.Repositories;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Beer.Identity.UnitTests.Commands.Clients
{
    public class DeleteClientCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean repoResult)
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientExists(clientSystemId)).ReturnsAsync(true).Verifiable();
            repoMock.Setup(x => x.DeleteClient(clientSystemId)).ReturnsAsync(repoResult).Verifiable();

            var handler = new DeleteClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<DeleteClientCommandHandler>>());

            Boolean result = await handler.Handle(new DeleteClientCommand(clientSystemId), CancellationToken.None);
            Assert.Equal(repoResult, result);

            repoMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_ClientNotFound()
        {
            Random random = new Random();
            Guid clientSystemId = random.NextGuid();

            var repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.CheckIfClientExists(clientSystemId)).ReturnsAsync(false).Verifiable();

            var handler = new DeleteClientCommandHandler(repoMock.Object,
                Mock.Of<ILogger<DeleteClientCommandHandler>>());

            Boolean result = await handler.Handle(new DeleteClientCommand(clientSystemId), CancellationToken.None);
            Assert.False(result);

            repoMock.Verify();
        }
    }
}
