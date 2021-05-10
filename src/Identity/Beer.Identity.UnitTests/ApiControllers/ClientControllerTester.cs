using Beer.Identity.ApiControllers;
using Beer.Identity.Commands.Clients;
using Beer.Identity.Infrastructure.Repositories;
using Beer.TestHelper;
using MediatR;
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
using static Beer.Identity.Shared.Responses.ClientResponses.V1;

namespace Beer.Identity.UnitTests.ApiControllers
{
    public class ClientControllerTester
    {
        [Fact]
        public async Task GetAllClients()
        {
            List<ClientOverview> expectedResult = new List<ClientOverview>();

            Mock<IClientRepository> repoMock = new Mock<IClientRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetAllClientsSortedByName()).ReturnsAsync(expectedResult).Verifiable();

            var controller = new ClientController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                repoMock.Object,
                Mock.Of<ILogger<ClientController>>()
                );

            var actionResult = await controller.GetAllClients();
            var actual = actionResult.EnsureOkObjectResult<IEnumerable<ClientOverview>>(true);
            Assert.Equal(expectedResult, actual);

            repoMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateClient(Boolean mediatorResult)
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            var request = new CreateClientRequest
            {

            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<CreateClientCommand>(y =>
           y.Request == request), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult == false ? new Guid?() : id).Verifiable();

            var controller = new ClientController(
                mediatorMock.Object,
                Mock.Of<IClientRepository>(MockBehavior.Strict),
                Mock.Of<ILogger<ClientController>>()
                );

            var actionResult = await controller.CreateClient(request);
            if (mediatorResult == true)
            {
                var actual = actionResult.EnsureOkObjectResult<Guid>(true);
                Assert.Equal(id, actual);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateClient(Boolean mediatorResult)
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            var request = new UpdateClientRequest
            {

            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<UpdateClientCommand>(y =>
           y.Request == request && y.Request.SystemId == id), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new ClientController(
                mediatorMock.Object,
                Mock.Of<IClientRepository>(MockBehavior.Strict),
                Mock.Of<ILogger<ClientController>>()
                );

            var actionResult = await controller.UpdateClient(id, request);
            if (mediatorResult == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteClient(Boolean mediatorResult)
        {
            Random random = new Random();
            Guid id = random.NextGuid();

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<DeleteClientCommand>(y =>
           y.SystemId == id), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new ClientController(
                mediatorMock.Object,
                Mock.Of<IClientRepository>(MockBehavior.Strict),
                Mock.Of<ILogger<ClientController>>()
                );

            var actionResult = await controller.DeleteClient(id);
            if (mediatorResult == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }
    }
}
