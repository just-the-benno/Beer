using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using Beer.DaAPI.Service.API.ApiControllers;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv4Leases;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Shared.Responses.DHCPv4LeasesResponses.V1;

namespace Beer.DaAPI.UnitTests.Host.ApiControllers
{
    public class DHCPv4LeaseControllerTester
    {
        protected DHCPv4RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            var scope = new DHCPv4RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(), factoryMock.Object);

            return scope;
        }

        private class LeaseOverviewEqualityComparer : IEqualityComparer<DHCPv4LeaseOverview>
        {
            public bool Equals([AllowNull] DHCPv4LeaseOverview x, [AllowNull] DHCPv4LeaseOverview y) =>
                 x.Address == y.Address &&
                   ByteHelper.AreEqual(x.MacAddress, y.MacAddress) == true &&
                   x.ExpectedEnd == y.ExpectedEnd &&
                   x.Id == y.Id &&
                   x.Scope.Name == y.Scope.Name &&
                   x.Scope.Id == y.Scope.Id &&
                   x.Started == y.Started &&
                   x.State == y.State &&
                   ByteHelper.AreEqual(x.UniqueIdentifier, y.UniqueIdentifier) == true;

            public int GetHashCode([DisallowNull] DHCPv4LeaseOverview obj) => obj.Id.GetHashCode();
        }

        [Fact]
        public void GetLeasesByScope_OnlyDirect()
        {
            Random random = new Random();
            Guid scopeId = random.NextGuid();
            String scopeName = "Testscope";

            DHCPv4LeaseOverview activeLeaseWithoutPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = random.NextBytes(10),
                State = LeaseStates.Active,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = scopeId,
                    Name = scopeName,
                }
            };

            DHCPv4LeaseOverview expiredLeaseWithPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = Array.Empty<Byte>(),
                State = LeaseStates.Inactive,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = scopeId,
                    Name = scopeName,
                }
            };

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    Name = scopeName,
                    Id = scopeId,
                }),
                 new DHCPv4LeaseCreatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    Address = IPv4Address.FromString(expiredLeaseWithPrefix.Address.ToString()),
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(expiredLeaseWithPrefix.MacAddress).GetBytes(),
                    ScopeId = scopeId,
                    StartedAt = expiredLeaseWithPrefix.Started,
                    ValidUntil = expiredLeaseWithPrefix.ExpectedEnd,
                    UniqueIdentifier = null,
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = scopeId,
                },
                new DHCPv4LeaseExpiredEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = scopeId,
                },
                   new DHCPv4LeaseCreatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    Address = IPv4Address.FromString(activeLeaseWithoutPrefix.Address.ToString()),
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(activeLeaseWithoutPrefix.MacAddress).GetBytes(),
                    ScopeId = scopeId,
                    StartedAt = activeLeaseWithoutPrefix.Started,
                    ValidUntil = activeLeaseWithoutPrefix.ExpectedEnd,
                    UniqueIdentifier = activeLeaseWithoutPrefix.UniqueIdentifier
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    ScopeId = scopeId,
                },
            });;


            var controller = new DHCPv4LeaseController(rootScope, Mock.Of<IMediator>(MockBehavior.Strict), Mock.Of<IDHCPv4ReadStore>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv4LeaseController>>());

            var actionResult = controller.GetCurrentLeasesByScope(scopeId);
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv4LeaseOverview>>(true);

            Assert.Equal(new[] { activeLeaseWithoutPrefix, expiredLeaseWithPrefix }, result, new LeaseOverviewEqualityComparer());

        }

        [Fact]
        public void GetLeasesByScope_WithParents()
        {
            Random random = new Random();
            Guid grantParentId = random.NextGuid();
            String grantParentScopeName = "Grant parent";

            Guid parentId = random.NextGuid();
            String parentScopeName = "Parent";

            Guid childId = random.NextGuid();
            String childScopeName = "Child";

            DHCPv4LeaseOverview activeLeaseWithoutPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = random.NextBytes(10),
                State = LeaseStates.Active,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = childId,
                    Name = childScopeName,
                }
            };

            DHCPv4LeaseOverview expiredLeaseWithPrefix = new DHCPv4LeaseOverview
            {
                Address = random.GetIPv4Address().ToString(),
                MacAddress = random.NextBytes(6),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                UniqueIdentifier = Array.Empty<Byte>(),
                State = LeaseStates.Inactive,
                Scope = new DHCPv4ScopeOverview
                {
                    Id = grantParentId,
                    Name = grantParentScopeName,
                }
            };

            DHCPv4RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    Name = grantParentScopeName,
                    Id = grantParentId,
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Name = parentScopeName,
                    Id = parentId,
                    ParentId = grantParentId,
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(new DHCPv4ScopeCreateInstruction
                {
                    Name = childScopeName,
                    Id = childId,
                    ParentId = parentId,
                }),
                 new DHCPv4LeaseCreatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    Address = IPv4Address.FromString(expiredLeaseWithPrefix.Address.ToString()),
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(expiredLeaseWithPrefix.MacAddress).GetBytes(),
                    ScopeId = grantParentId,
                    StartedAt = expiredLeaseWithPrefix.Started,
                    ValidUntil = expiredLeaseWithPrefix.ExpectedEnd,
                    UniqueIdentifier = null
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = grantParentId,
                },
                new DHCPv4LeaseExpiredEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = grantParentId,
                },
                   new DHCPv4LeaseCreatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    Address = IPv4Address.FromString(activeLeaseWithoutPrefix.Address),
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(activeLeaseWithoutPrefix.MacAddress).GetBytes(),
                    ScopeId = childId,
                    StartedAt = activeLeaseWithoutPrefix.Started,
                    ValidUntil = activeLeaseWithoutPrefix.ExpectedEnd,
                    UniqueIdentifier = activeLeaseWithoutPrefix.UniqueIdentifier
                },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    ScopeId = childId,
                },
            });


            var controller = new DHCPv4LeaseController(rootScope, Mock.Of<IMediator>(MockBehavior.Strict), Mock.Of<IDHCPv4ReadStore>(MockBehavior.Strict),Mock.Of<ILogger<DHCPv4LeaseController>>());

            var actionResult = controller.GetCurrentLeasesByScope(grantParentId, true);
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv4LeaseOverview>>(true);

            Assert.Equal(new[] { activeLeaseWithoutPrefix, expiredLeaseWithPrefix }, result, new LeaseOverviewEqualityComparer());

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CancelLease(Boolean mediatorResult)
        {
            Random random = new Random();

            Guid leaseId = random.NextGuid();

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(x => x.Send(It.Is<CancelDHCPv4LeaseCommand>(y => y.LeaseId == leaseId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult).Verifiable();

            var controller = new DHCPv4LeaseController(GetRootScope(), mediatorMock.Object, Mock.Of<IDHCPv4ReadStore>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv4LeaseController>>());
            var actionResult = await controller.CancelLease(leaseId);

            if(mediatorResult == true)
            {
                Boolean result = actionResult.EnsureOkObjectResult<Boolean>(true);
                Assert.True(result);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to delete lease");
            }

            mediatorMock.Verify();
        }
    }
}
