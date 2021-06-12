using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv6Scopes;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
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
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6ScopeEvents;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using Beer.DaAPI.Service.TestHelper;

namespace Beer.DaAPI.UnitTests.Host.Commands.DHCPv6Scopes
{
    public class UpdateDHCPv6ScopeParentCommandHandlerTester
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_WithParentId(Boolean storeResult)
        {
            Random random = new();

            Guid scopeId = random.NextGuid();
            Guid parentId = random.NextGuid();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] { new DHCPv6ScopeAddedEvent {
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = parentId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv6ScopeProperties(),
                    AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
                }
            },
            new DHCPv6ScopeAddedEvent {
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = scopeId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv6ScopeProperties(),
                    AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
                }
            },
           });

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storeResult).Verifiable();

            var command = new UpdateDHCPv6ScopeParentCommand(scopeId, parentId);
            var handler = new UpdateDHCPv6ScopeParentCommandHandler(storageMock.Object, rootScope, Mock.Of<ILogger<UpdateDHCPv6ScopeParentCommandHandler>>());
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

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] { new DHCPv6ScopeAddedEvent {
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = parentId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv6ScopeProperties(),
                    AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
                }
            },
            new DHCPv6ScopeAddedEvent {
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = scopeId,
                    ParentId = parentId,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv6ScopeProperties(),
                    AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
                }
            },
           });

            Mock<IDHCPv6StorageEngine> storageMock = new Mock<IDHCPv6StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storeResult).Verifiable();

            var command = new UpdateDHCPv6ScopeParentCommand(scopeId, null);
            var handler = new UpdateDHCPv6ScopeParentCommandHandler(storageMock.Object, rootScope, Mock.Of<ILogger<UpdateDHCPv6ScopeParentCommandHandler>>());
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

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] {
            new DHCPv6ScopeAddedEvent {
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = scopeId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv6ScopeProperties(),
                    AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
                }
            },
           });

            var command = new UpdateDHCPv6ScopeParentCommand(scopeId, parentId);
            var handler = new UpdateDHCPv6ScopeParentCommandHandler(Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict), rootScope, Mock.Of<ILogger<UpdateDHCPv6ScopeParentCommandHandler>>());
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

            Mock<IScopeResolver<DHCPv6Packet, IPv6Address>> resolverMock = new Mock<IScopeResolver<DHCPv6Packet, IPv6Address>>(MockBehavior.Strict);

            Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv6Packet, IPv6Address>>();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(resolverMock.Object).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            DHCPv6RootScope rootScope = new DHCPv6RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);
            rootScope.Load(new DomainEvent[] {
            new DHCPv6ScopeAddedEvent {
                Instructions = new DHCPv6ScopeCreateInstruction
                {
                    Name = random.GetAlphanumericString(),
                    Description = random.GetAlphanumericString(),
                    Id = parentId,
                    ParentId = null,
                    ResolverInformation = new CreateScopeResolverInformation
                    {
                        Typename = resolverName,
                    },
                    ScopeProperties = new DHCPv6ScopeProperties(),
                    AddressProperties =  DHCPv6ScopeAddressProperties.Empty,
                }
            },
           });

            var command = new UpdateDHCPv6ScopeParentCommand(scopeId, parentId);
            var handler = new UpdateDHCPv6ScopeParentCommandHandler(Mock.Of<IDHCPv6StorageEngine>(MockBehavior.Strict), rootScope, Mock.Of<ILogger<UpdateDHCPv6ScopeParentCommandHandler>>());
            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result);

            scopeResolverMock.Verify();
        }
    }
}
