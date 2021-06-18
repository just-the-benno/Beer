using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes;
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
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv4Leases;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv6Leases;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;

namespace Beer.DaAPI.UnitTests.Host.Commands.DHCPv6Leases
{
    public class CancelDHCPv6LeaseCommandHandlerTester
    {
        [Fact]
        public async Task Handle_LeaseNotFound()
        {
            Random random = new Random();

            Guid leaseId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[]{
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = scopeId,
                    }
                },
                new DHCPv6LeaseEvents.DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    ClientIdentifier = new UUIDDUID(random.NextGuid())
                },
            });

            var command = new CancelDHCPv6LeaseCommand(scopeId);

            var handler = new CancelDHCPv6LeaseCommandHandler(Mock.Of<IDHCPv6StorageEngine>(), Mock.Of<IServiceBus>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<CancelDHCPv6LeaseCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_LeaseFound_NotCancelable()
        {
            Random random = new Random();

            Guid leaseId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[]{
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = scopeId,
                    }
                },
                new DHCPv6LeaseEvents.DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    ClientIdentifier = new UUIDDUID(random.NextGuid())
                },
                new DHCPv6LeaseEvents.DHCPv6LeaseCanceledEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });

            var command = new CancelDHCPv6LeaseCommand(leaseId);

            var handler = new CancelDHCPv6LeaseCommandHandler(Mock.Of<IDHCPv6StorageEngine>(), Mock.Of<IServiceBus>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<CancelDHCPv6LeaseCommandHandler>>());

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

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[]{
                new DHCPv6ScopeAddedEvent
                {
                    Instructions = new DHCPv6ScopeCreateInstruction
                    {
                        Id = scopeId,
                    }
                },
                new DHCPv6LeaseEvents.DHCPv6LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                    ClientIdentifier = new UUIDDUID(random.NextGuid())
                },
                new DHCPv6LeaseEvents.DHCPv6LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storageResult).Verifiable();

            var command = new CancelDHCPv6LeaseCommand(leaseId);

            var handler = new CancelDHCPv6LeaseCommandHandler(storageMock.Object, Mock.Of<IServiceBus>(MockBehavior.Strict), rootScope,
                Mock.Of<ILogger<CancelDHCPv6LeaseCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.Equal(storageResult, result);
        }
    }
}
