using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv4Scopes;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
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
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using Beer.DaAPI.Service.TestHelper;

namespace Beer.DaAPI.UnitTests.Host.Commands.DHCPv4Scopes
{
    public class UpdateDHCPv4ScopeParentCommandHandlerTester
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_WithParentId(Boolean storeResult)
        {
            Random random = new ();

            Guid scopeId = random.NextGuid();
            Guid parentId = random.NextGuid();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] { new DHCPv4ScopeAddedEvent {
                Instructions = new DHCPv4ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = parentId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv4ScopeProperties(),
                    AddressProperties =  DHCPv4ScopeAddressProperties.Empty,
                }
            },
            new DHCPv4ScopeAddedEvent {
                Instructions = new DHCPv4ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = scopeId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv4ScopeProperties(),
                    AddressProperties =  DHCPv4ScopeAddressProperties.Empty,
                }
            },
           });

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storeResult).Verifiable();

            var command = new UpdateDHCPv4ScopeParentCommand(scopeId, parentId);
            var handler = new UpdateDHCPv4ScopeParentCommandHandler(storageMock.Object, rootScope, Mock.Of<ILogger<UpdateDHCPv4ScopeParentCommandHandler>>());
            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.Equal(storeResult, result);

            var parentScope = rootScope.GetScopeById(parentId);
            var childIds = parentScope.GetChildIds(true);

            Assert.Single(childIds);
            Assert.Equal(scopeId, childIds.First());

            scopeResolverMock.Verify();
            storageMock.Verify();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_UpdateToRoot(Boolean storeResult)
        {
            Random random = new();

            Guid scopeId = random.NextGuid();
            Guid parentId = random.NextGuid();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] { new DHCPv4ScopeAddedEvent {
                Instructions = new DHCPv4ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = parentId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv4ScopeProperties(),
                    AddressProperties =  DHCPv4ScopeAddressProperties.Empty,
                }
            },
            new DHCPv4ScopeAddedEvent {
                Instructions = new DHCPv4ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = scopeId,
                    ParentId = parentId,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv4ScopeProperties(),
                    AddressProperties =  DHCPv4ScopeAddressProperties.Empty,
                }
            },
           });

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storeResult).Verifiable();

            var command = new UpdateDHCPv4ScopeParentCommand(scopeId, null);
            var handler = new UpdateDHCPv4ScopeParentCommandHandler(storageMock.Object, rootScope, Mock.Of<ILogger<UpdateDHCPv4ScopeParentCommandHandler>>());
            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.Equal(storeResult, result);

            var parentScope = rootScope.GetScopeById(parentId);
            var childIds = parentScope.GetChildIds(true);

            Assert.Empty(childIds);

            var childScope = rootScope.GetScopeById(scopeId);
            Assert.Null(childScope.ParentScope);

            scopeResolverMock.Verify();
            storageMock.Verify();
        }

        [Fact]
        public async Task Handle_WithParentId_ParentNotFound()
        {
            Random random = new();

            Guid scopeId = random.NextGuid();
            Guid parentId = random.NextGuid();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] { 
            new DHCPv4ScopeAddedEvent {
                Instructions = new DHCPv4ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = scopeId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv4ScopeProperties(),
                    AddressProperties =  DHCPv4ScopeAddressProperties.Empty,
                }
            },
           });

            var command = new UpdateDHCPv4ScopeParentCommand(scopeId, parentId);
            var handler = new UpdateDHCPv4ScopeParentCommandHandler(Mock.Of<IDHCPv4StorageEngine>(MockBehavior.Strict), rootScope, Mock.Of<ILogger<UpdateDHCPv4ScopeParentCommandHandler>>());
            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);

            scopeResolverMock.Verify();
        }

        [Fact]
        public async Task Handle_ScopeNotFound()
        {
            Random random = new();

            Guid scopeId = random.NextGuid();
            Guid parentId = random.NextGuid();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] {
            new DHCPv4ScopeAddedEvent {
                Instructions = new DHCPv4ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = parentId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv4ScopeProperties(),
                    AddressProperties =  DHCPv4ScopeAddressProperties.Empty,
                }
            },
           });

            var command = new UpdateDHCPv4ScopeParentCommand(scopeId, parentId);
            var handler = new UpdateDHCPv4ScopeParentCommandHandler(Mock.Of<IDHCPv4StorageEngine>(MockBehavior.Strict), rootScope, Mock.Of<ILogger<UpdateDHCPv4ScopeParentCommandHandler>>());
            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);

            scopeResolverMock.Verify();
        }
    }
}
