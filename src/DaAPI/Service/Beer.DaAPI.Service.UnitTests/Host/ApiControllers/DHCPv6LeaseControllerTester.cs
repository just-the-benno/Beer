using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Service.API.ApiControllers;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv6Leases;
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
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
using static Beer.DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;

namespace Beer.DaAPI.UnitTests.Host.ApiControllers
{
    public class DHCPv6LeaseControllerTester
    {
        protected DHCPv6RootScope GetRootScope()
        {
            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv6RootScope>>());

            var scope = new DHCPv6RootScope(Guid.NewGuid(), Mock.Of<IScopeResolverManager<DHCPv6Packet, IPv6Address>>(), factoryMock.Object);

            return scope;
        }

        private class LeaseOverviewEqualityComparer : IEqualityComparer<DHCPv6LeaseOverview>
        {
            public bool Equals([AllowNull] DHCPv6LeaseOverview x, [AllowNull] DHCPv6LeaseOverview y)
            {
                if (x.Address == y.Address &&
                   x.ClientIdentifier == y.ClientIdentifier &&
                   x.ExpectedEnd == y.ExpectedEnd &&
                   x.Id == y.Id &&
                   x.Scope.Name == y.Scope.Name &&
                   x.Scope.Id == y.Scope.Id &&
                   x.Started == y.Started &&
                   x.State == y.State &&
                   ByteHelper.AreEqual(x.UniqueIdentifier, y.UniqueIdentifier) == true
                    )
                {
                    if (x.Prefix != null && y.Prefix != null)
                    {
                        return x.Prefix.Address == y.Prefix.Address && x.Prefix.Mask == y.Prefix.Mask;
                    }
                    else if (x.Prefix == null && y.Prefix == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode([DisallowNull] DHCPv6LeaseOverview obj) => obj.Id.GetHashCode();
        }

        [Fact]
        public void GetLeasesByScope_OnlyDirect()
        {
            Random random = new Random();
            Guid scopeId = random.NextGuid();
            String scopeName = "Testscope";

            DHCPv6LeaseOverview activeLeaseWithoutPrefix = new DHCPv6LeaseOverview
            {
                Address = random.GetIPv6Address().ToString(),
                ClientIdentifier = new UUIDDUID(random.NextGuid()),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                Prefix = null,
                UniqueIdentifier = random.NextBytes(10),
                State = LeaseStates.Active,
                Scope = new ScopeOverview
                {
                    Id = scopeId,
                    Name = scopeName,
                }
            };

            DHCPv6LeaseOverview expiredLeaseWithPrefix = new DHCPv6LeaseOverview
            {
                Address = random.GetIPv6Address().ToString(),
                ClientIdentifier = new UUIDDUID(random.NextGuid()),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                Prefix = new PrefixOverview
                {
                    Address = IPv6Address.FromString("fe70::").ToString(),
                    Mask = 64,
                },
                UniqueIdentifier = Array.Empty<Byte>(),
                State = LeaseStates.Inactive,
                Scope = new ScopeOverview
                {
                    Id = scopeId,
                    Name = scopeName,
                }
            };

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = scopeName,
                    Id = scopeId,
                }),
                 new DHCPv6LeaseCreatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    Address = IPv6Address.FromString(expiredLeaseWithPrefix.Address.ToString()),
                    ClientIdentifier = expiredLeaseWithPrefix.ClientIdentifier,
                    IdentityAssocationId = random.NextUInt32(),
                    ScopeId = scopeId,
                    StartedAt = expiredLeaseWithPrefix.Started,
                    ValidUntil = expiredLeaseWithPrefix.ExpectedEnd,
                    HasPrefixDelegation = true,
                    PrefixLength = expiredLeaseWithPrefix.Prefix.Mask,
                    DelegatedNetworkAddress = IPv6Address.FromString(expiredLeaseWithPrefix.Prefix.Address),
                    IdentityAssocationIdForPrefix = random.NextUInt32(),
                    UniqueIdentiifer = null
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = scopeId,
                },
                new DHCPv6LeaseExpiredEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = scopeId,
                },
                   new DHCPv6LeaseCreatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    Address = IPv6Address.FromString(activeLeaseWithoutPrefix.Address.ToString()),
                    ClientIdentifier = activeLeaseWithoutPrefix.ClientIdentifier,
                    IdentityAssocationId = random.NextUInt32(),
                    ScopeId = scopeId,
                    StartedAt = activeLeaseWithoutPrefix.Started,
                    ValidUntil = activeLeaseWithoutPrefix.ExpectedEnd,
                    HasPrefixDelegation = false,
                    UniqueIdentiifer = activeLeaseWithoutPrefix.UniqueIdentifier
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    ScopeId = scopeId,
                },
            });


            var controller = new DHCPv6LeaseController(rootScope, Mock.Of<IMediator>(MockBehavior.Strict), Mock.Of<IDHCPv6ReadStore>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv6LeaseController>>());

            var actionResult = controller.GetLeasesByScope(scopeId);
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv6LeaseOverview>>(true);

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

            DHCPv6LeaseOverview activeLeaseWithoutPrefix = new DHCPv6LeaseOverview
            {
                Address = random.GetIPv6Address().ToString(),
                ClientIdentifier = new UUIDDUID(random.NextGuid()),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                Prefix = null,
                UniqueIdentifier = random.NextBytes(10),
                State = LeaseStates.Active,
                Scope = new ScopeOverview
                {
                    Id = childId,
                    Name = childScopeName,
                }
            };

            DHCPv6LeaseOverview expiredLeaseWithPrefix = new DHCPv6LeaseOverview
            {
                Address = random.GetIPv6Address().ToString(),
                ClientIdentifier = new UUIDDUID(random.NextGuid()),
                ExpectedEnd = DateTime.UtcNow.AddHours(random.Next(10, 20)),
                Started = DateTime.UtcNow.AddHours(-random.Next(10, 20)),
                Id = random.NextGuid(),
                Prefix = new PrefixOverview
                {
                    Address = IPv6Address.FromString("fe70::").ToString(),
                    Mask = 64,
                },
                UniqueIdentifier = Array.Empty<Byte>(),
                State = LeaseStates.Inactive,
                Scope = new ScopeOverview
                {
                    Id = grantParentId,
                    Name = grantParentScopeName,
                }
            };

            DHCPv6RootScope rootScope = GetRootScope();
            rootScope.Load(new List<DomainEvent>{ new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(
                new DHCPv6ScopeCreateInstruction
                {
                    Name = grantParentScopeName,
                    Id = grantParentId,
                }),
                new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Name = parentScopeName,
                    Id = parentId,
                    ParentId = grantParentId,
                }),
                new DHCPv6ScopeEvents.DHCPv6ScopeAddedEvent(new DHCPv6ScopeCreateInstruction
                {
                    Name = childScopeName,
                    Id = childId,
                    ParentId = parentId,
                }),
                 new DHCPv6LeaseCreatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    Address = IPv6Address.FromString(expiredLeaseWithPrefix.Address.ToString()),
                    ClientIdentifier = expiredLeaseWithPrefix.ClientIdentifier,
                    IdentityAssocationId = random.NextUInt32(),
                    ScopeId = grantParentId,
                    StartedAt = expiredLeaseWithPrefix.Started,
                    ValidUntil = expiredLeaseWithPrefix.ExpectedEnd,
                    HasPrefixDelegation = true,
                    PrefixLength = expiredLeaseWithPrefix.Prefix.Mask,
                    DelegatedNetworkAddress = IPv6Address.FromString(expiredLeaseWithPrefix.Prefix.Address.ToString()),
                    IdentityAssocationIdForPrefix = random.NextUInt32(),
                    UniqueIdentiifer = null
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = grantParentId,
                },
                new DHCPv6LeaseExpiredEvent
                {
                    EntityId = expiredLeaseWithPrefix.Id,
                    ScopeId = grantParentId,
                },
                   new DHCPv6LeaseCreatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    Address = IPv6Address.FromString(activeLeaseWithoutPrefix.Address),
                    ClientIdentifier = activeLeaseWithoutPrefix.ClientIdentifier,
                    IdentityAssocationId = random.NextUInt32(),
                    ScopeId = childId,
                    StartedAt = activeLeaseWithoutPrefix.Started,
                    ValidUntil = activeLeaseWithoutPrefix.ExpectedEnd,
                    HasPrefixDelegation = false,
                    UniqueIdentiifer = activeLeaseWithoutPrefix.UniqueIdentifier
                },
                new DHCPv6LeaseActivatedEvent
                {
                    EntityId = activeLeaseWithoutPrefix.Id,
                    ScopeId = childId,
                },
            });


            var controller = new DHCPv6LeaseController(rootScope, Mock.Of<IMediator>(MockBehavior.Strict), Mock.Of<IDHCPv6ReadStore>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv6LeaseController>>());

            var actionResult = controller.GetLeasesByScope(grantParentId, true);
            var result = actionResult.EnsureOkObjectResult<IEnumerable<DHCPv6LeaseOverview>>(true);

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
                .Setup(x => x.Send(It.Is<CancelDHCPv6LeaseCommand>(y => y.LeaseId == leaseId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult).Verifiable();

            var controller = new DHCPv6LeaseController(GetRootScope(), mediatorMock.Object, Mock.Of<IDHCPv6ReadStore>(MockBehavior.Strict), Mock.Of<ILogger<DHCPv6LeaseController>>());
            var actionResult = await controller.CancelLease(leaseId);

            if (mediatorResult == true)
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
