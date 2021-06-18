using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv4Scopes;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
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
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv4Leases;

namespace Beer.DaAPI.UnitTests.Host.Commands.DHCPv4Leases
{
    public class CancelDHCPv4LeaseCommandHandlerTester
    {
        [Fact]
        public async Task Handle_LeaseNotFound()
        {
            Random random = new Random();

            Guid leaseId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[]{
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = scopeId,
                    }
                },
                new DHCPv4LeaseEvents.DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });

            var command = new CancelDHCPv4LeaseCommand(scopeId);

            var handler = new CancelDHCPv4LeaseCommandHandler(Mock.Of<IDHCPv4StorageEngine>(), Mock.Of<IServiceBus>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<CancelDHCPv4LeaseCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_LeaseFound_NotCancelable()
        {
            Random random = new Random();

            Guid leaseId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[]{
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = scopeId,
                    }
                },
                new DHCPv4LeaseEvents.DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseEvents.DHCPv4LeaseCanceledEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });

            var command = new CancelDHCPv4LeaseCommand(leaseId);

            var handler = new CancelDHCPv4LeaseCommandHandler(Mock.Of<IDHCPv4StorageEngine>(), Mock.Of<IServiceBus>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<CancelDHCPv4LeaseCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_Cancel(Boolean storageResult)
        {
            Random random = new Random();

            Guid leaseId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[]{
                new DHCPv4ScopeAddedEvent
                {
                    Instructions = new DHCPv4ScopeCreateInstruction
                    {
                        Id = scopeId,
                    }
                },
                new DHCPv4LeaseEvents.DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseEvents.DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storageResult).Verifiable();

            var command = new CancelDHCPv4LeaseCommand(leaseId);

            var handler = new CancelDHCPv4LeaseCommandHandler(storageMock.Object, Mock.Of<IServiceBus>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<CancelDHCPv4LeaseCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.Equal(storageResult, result);
        }
    }
}
