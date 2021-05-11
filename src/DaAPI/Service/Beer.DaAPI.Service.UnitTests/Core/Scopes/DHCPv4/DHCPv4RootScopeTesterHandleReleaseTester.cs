using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using Beer.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static Beer.DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents.DHCPv4ReleaseHandledEvent;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4RootScopeTesterHandleReleaseTester : DHCPv4RootScopeTesterBase
    {
        private void CheckHandeledEvent(
          Int32 index, ReleaseError error,
          DHCPv4Packet requestPacket,
          DHCPv4RootScope rootScope)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4ReleaseHandledEvent>(changes.ElementAt(index));

            DHCPv4ReleaseHandledEvent handeledEvent = (DHCPv4ReleaseHandledEvent)changes.ElementAt(index);
            Assert.Equal(requestPacket, handeledEvent.Request);
            Assert.Equal(DHCPv4Packet.Empty, handeledEvent.Response);
            Assert.Equal(error, handeledEvent.Error);
            if (error == ReleaseError.NoError)
            {
                Assert.True(handeledEvent.WasSuccessfullHandled);
            }
            else
            {
                Assert.False(handeledEvent.WasSuccessfullHandled);
            }
        }

        private static void CheckLeaseReleasedEvent(Int32 index,
         Guid scopeId, DHCPv4RootScope rootScope,
         Guid leaseId
         )
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);

            Assert.IsAssignableFrom<DHCPv4LeaseReleasedEvent>(changes.ElementAt(index));

            DHCPv4LeaseReleasedEvent createdEvent = (DHCPv4LeaseReleasedEvent)changes.ElementAt(index);
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.Equal(leaseId, createdEvent.EntityId);
        }

        private void CheckIfLeaseIsRelease(DHCPv4Lease lease)
        {
            Assert.NotEqual(DHCPv4Lease.Empty, lease);
            Assert.False(lease.IsActive());
            Assert.Equal(LeaseStates.Released, lease.State);
        }

        [Fact]
        public void HandleRelease_LeaseExists_AddressIsActive()
        {
            Random random = new Random();
            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Release)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
               new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1)
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(clientMacAdress).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });
            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);

            DHCPv4Packet result = rootScope.HandleRelease(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            CheckIfLeaseIsRelease(lease);

            CheckEventAmount(2, rootScope);
            CheckLeaseReleasedEvent(0, scopeId, rootScope, leaseId);
            CheckHandeledEvent(1, ReleaseError.NoError, requestPacket, rootScope);
        }

        [Fact]
        public void HandleRelease_LeaseExists_AddressIsActive_NotSentDiscover()
        {
            Random random = new Random();
            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Release)
            );

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
               headerInformation, clientMacAdress, (UInt32)random.Next(),
               IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
               DHCPv4PacketFlags.Broadcast,
               new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover),
               new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromIdentifierValue("random test client"))
           );

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
               new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(18),
                        reuseAddressIfPossible: true,
                        acceptDecline: true,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next,
                        supportDirectUnicast: true,
                        maskLength: 24
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromIdentifierValue("random test client").GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
                new DHCPv4LeaseActivatedEvent
                {
                    EntityId = leaseId,
                    ScopeId = scopeId,
                },
            });
            
            DHCPv4Lease existingLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);

            DHCPv4Packet releaseResult = rootScope.HandleRelease(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, releaseResult);

            CheckIfLeaseIsRelease(existingLease);

            CheckEventAmount(2, rootScope);
            CheckLeaseReleasedEvent(0, scopeId, rootScope, leaseId);
            CheckHandeledEvent(1, ReleaseError.NoError, requestPacket, rootScope);

            DHCPv4Packet discoveryResult = rootScope.HandleDiscover(discoverPacket);
            IPv4Address newAssignedAddress = IPv4Address.FromString("192.168.178.2");

            Assert.Equal(newAssignedAddress, discoveryResult.YourIPAdress);
        }

        [Fact]
        public void HandleRelease_LeaseExists_AddressIsNotActive()
        {
            Random random = new Random();
            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(leasedAddress, IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Release)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
               new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1)
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(clientMacAdress).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            });

            DHCPv4Packet result = rootScope.HandleRelease(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(LeaseStates.Pending, lease.State);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, ReleaseError.LeaseNotActive, requestPacket, rootScope);
        }

        [Fact]
        public void HandleRelease_LeaseNotExists()
        {
            Random random = new Random();
            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.FromString("192.168.178.9"), IPv4Address.FromString("192.168.178.1"));

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet requestPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Release)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
               new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1)
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(clientMacAdress).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            });

            DHCPv4Packet result = rootScope.HandleRelease(requestPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            DHCPv4Lease lease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);
            Assert.Equal(LeaseStates.Pending, lease.State);

            CheckEventAmount(1, rootScope);
            CheckHandeledEvent(0, ReleaseError.NoLeaseFound, requestPacket, rootScope);
        }
    }
}
