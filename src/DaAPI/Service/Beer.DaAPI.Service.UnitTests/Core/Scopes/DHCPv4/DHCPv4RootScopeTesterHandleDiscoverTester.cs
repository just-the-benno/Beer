﻿using Beer.DaAPI.Core.Common;
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
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4Lease;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4ScopeEvents;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv4
{
    public class DHCPv4RootScopeTesterHandleDiscoverTester : DHCPv4RootScopeTesterBase
    {
        private void CheckPacket(IPv4Address expectedAdress, DHCPv4Packet result)
        {
            Assert.NotNull(result);
            Assert.NotEqual(DHCPv4Packet.Empty, result);
            Assert.True(result.IsValid);

            Assert.Equal(expectedAdress, result.YourIPAdress);
            Assert.Equal(DHCPv4MessagesTypes.Offer, result.MessageType);
        }

        private static void CheckLeaseRenewdEvent(
    Guid scopeId,
    DHCPv4RootScope rootScope, DHCPv4Lease lease, TimeSpan renewalTime, TimeSpan preferredLifetime)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();
            Assert.NotNull(changes);
            Assert.Equal(2, changes.Count());

            Assert.IsAssignableFrom<DHCPv4LeaseRenewedEvent>(changes.First());

            DHCPv4LeaseRenewedEvent createdEvent = (DHCPv4LeaseRenewedEvent)changes.First();
            Assert.NotNull(createdEvent);

            Assert.Equal(scopeId, createdEvent.ScopeId);
            Assert.NotEqual(Guid.Empty, lease.Id);

            Assert.Equal(lease.Id, createdEvent.EntityId);
            Assert.True(createdEvent.Reset);

            Assert.Equal(lease.End, createdEvent.End);
            Assert.True(Math.Abs((renewalTime - createdEvent.RenewSpan).TotalSeconds) < 20);
            Assert.True(Math.Abs((preferredLifetime - createdEvent.ReboundSpan).TotalSeconds) < 20);
        }

        private void CheckHandeledEvent(Int32 index, DHCPv4Packet discoverPacket, DHCPv4Packet result, DHCPv4RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4DiscoverHandledEvent>(changes.ElementAt(index));

            DHCPv4DiscoverHandledEvent handeledEvent = (DHCPv4DiscoverHandledEvent)changes.ElementAt(index);
            Assert.Equal(discoverPacket, handeledEvent.Request);
            Assert.Equal(result, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.True(handeledEvent.WasSuccessfullHandled);
        }

        private void CheckHandeledEventNotSuccessfull(Int32 index, DHCPv4Packet discoverPacket, DHCPv4RootScope rootScope, Guid scopeId)
        {
            IEnumerable<DomainEvent> changes = rootScope.GetChanges();

            Assert.IsAssignableFrom<DHCPv4DiscoverHandledEvent>(changes.ElementAt(index));

            DHCPv4DiscoverHandledEvent handeledEvent = (DHCPv4DiscoverHandledEvent)changes.ElementAt(index);
            Assert.Equal(discoverPacket, handeledEvent.Request);
            Assert.Equal(DHCPv4Packet.Empty, handeledEvent.Response);
            Assert.Equal(scopeId, handeledEvent.ScopeId);
            Assert.False(handeledEvent.WasSuccessfullHandled);
        }

        [Fact]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.0");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_DynamicInheritanceLeastime()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            DateTime temp = DateTime.Now.AddHours(2);
            DynamicRenewTime renewTime = DynamicRenewTime.WithDefaultRange(temp.Hour, temp.Minute);

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid parentId = random.NextGuid();
            Guid grantParentId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        renewTime,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Parent Testscope",
                    Id = grantParentId,
                }),
                new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.10"),
                        IPv4Address.FromString("192.168.178.15"),
                        Array.Empty<IPv4Address>()),
                    ResolverInformation = resolverInformations,
                    Name = "Child Testscope",
                    ParentId = grantParentId,
                    Id = parentId,
                }),
                 new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.12"),
                        IPv4Address.FromString("192.168.178.12"),
                        Array.Empty<IPv4Address>(),
                        maskLength: 24
                        ),
                    ResolverInformation = resolverInformations,
                    Name = "Child Testscope",
                    ParentId = parentId,
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.12");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, false);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease, null, false);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);

            Assert.Equal(lease.RenewSpan + TimeSpan.FromMinutes(renewTime.MinutesToRebound), lease.RebindingSpan);
            Assert.True((DateTime.UtcNow + (lease.RenewSpan + TimeSpan.FromMinutes(renewTime.MinutesToEndOfLife)) - lease.End).TotalMinutes < 60);
        }

        [Fact]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_WithDynamicRange()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            DateTime temp = DateTime.Now.AddHours(2);

            DynamicRenewTime renewTime = DynamicRenewTime.WithDefaultRange(temp.Hour, temp.Minute);

            TimeSpan minExpectedRenewSpan = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(-10)).Add(TimeSpan.FromSeconds(-60));
            TimeSpan maxExpectedRenewSpan = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(10)).Add(TimeSpan.FromSeconds(60));


            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        renewTime,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.0");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, false);

            Assert.True(lease.RenewSpan >= minExpectedRenewSpan);
            Assert.True(lease.RenewSpan <= maxExpectedRenewSpan);

            Assert.Equal(lease.RenewSpan + TimeSpan.FromMinutes(renewTime.MinutesToRebound), lease.RebindingSpan);
            Assert.True((DateTime.UtcNow + (lease.RenewSpan + TimeSpan.FromMinutes(renewTime.MinutesToEndOfLife)) - lease.End).TotalMinutes < 60);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease, null, false);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);

            var lifespanOption = result.GetOptionByIdentifier(DHCPv4OptionTypes.IPAddressLeaseTime) as DHCPv4PacketTimeSpanOption;
            Assert.NotNull(lifespanOption);
            Assert.True((lease.RenewSpan + TimeSpan.FromMinutes(renewTime.MinutesToEndOfLife) - lifespanOption.Value).TotalSeconds < 10);

            var rebindingOption = result.GetOptionByIdentifier(DHCPv4OptionTypes.RebindingTimeValue) as DHCPv4PacketTimeSpanOption;
            Assert.NotNull(rebindingOption);
            Assert.Equal(lease.RebindingSpan, rebindingOption.Value);

            var renewTimeOption = result.GetOptionByIdentifier(DHCPv4OptionTypes.RenewalTimeValue) as DHCPv4PacketTimeSpanOption;
            Assert.Equal(lease.RenewSpan, renewTimeOption.Value);
        }

        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_SingleAddressPool(
            DHCPv4ScopeAddressProperties.AddressAllocationStrategies allocationStrategy
            )
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.10"),
                        IPv4Address.FromString("192.168.178.10"),
                        new List<IPv4Address>(),
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.10");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_SingleAddressPool_ButInheriantance(
          DHCPv4ScopeAddressProperties.AddressAllocationStrategies allocationStrategy
          )
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid parentScopeId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.10"),
                        new List<IPv4Address>(),
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = parentScopeId,
                }),
                new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.8"),
                        IPv4Address.FromString("192.168.178.8"),
                        new List<IPv4Address>(),
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: allocationStrategy
                       ),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope Child",
                    ParentId = parentScopeId,
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.8");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_NoLeaseFound_ExcludedAddressUsedInChildScopes()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            var childScopeResolverInformation = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4PseudoResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> secondScopeResolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            secondScopeResolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);
            scopeResolverMock.Setup(x => x.InitializeResolver(childScopeResolverInformation)).Returns(secondScopeResolverMock.Object);

            Guid grandParentScopeId = random.NextGuid();
            Guid parentScopeId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.10"),
                        new List<IPv4Address>(),
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = grandParentScopeId,
                }),
                new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.5"),
                        new List<IPv4Address>(),
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next
                       ),
                    ResolverInformation = childScopeResolverInformation,
                    Name = "Testscope Child",
                    ParentId = grandParentScopeId,
                    Id = parentScopeId,
                }),
                new DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.1"),
                        new List<IPv4Address>(),
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next
                       ),
                    ResolverInformation = childScopeResolverInformation,
                    Name = "Testscope Child Child",
                    ParentId = parentScopeId,
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    Address = IPv4Address.FromString("192.168.178.1"),
                    ScopeId = scopeId,
                    EntityId = random.NextGuid(),
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(clientMacAdress).GetBytes(),
                    StartedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                }
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.6");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(grandParentScopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, grandParentScopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, grandParentScopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, grandParentScopeId);
        }

        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_FirstFreeAddress(
            DHCPv4ScopeAddressProperties.AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Byte avaiableAddress = random.NextByte();
            List<IPv4Address> excludedAddress = new List<IPv4Address>(255);
            for (int i = 0; i <= Byte.MaxValue; i++)
            {
                if (i != avaiableAddress)
                {
                    excludedAddress.Add(IPv4Address.FromBytes(192, 168, 178, (Byte)i));
                }
            }

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        excludedAddress,
                        leaseTime: TimeSpan.FromDays(1),
                        maskLength: 24,
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromBytes(192, 168, 178, avaiableAddress);

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFound_ReuseEnabled()
        {
            Random random = new();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            TimeSpan renewalTime = TimeSpan.FromHours(12);
            TimeSpan preferredLifetime = TimeSpan.FromHours(18);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: renewalTime,
                        preferredLifetime: preferredLifetime,
                        reuseAddressIfPossible: true,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(leasedAddress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, leasedAddress, scopeId, rootScope, leaseCreatedAt);
            Assert.Equal(leaseId, lease.Id);

            CheckEventAmount(2, rootScope);
            CheckLeaseRenewdEvent(scopeId, rootScope, lease, renewalTime, preferredLifetime);
            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFound_ReuseNotEnabled()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.1")
                        },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent(1, clientMacAdress, scopeId, rootScope, expectedAddress, lease);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFound_ReuseNotEnabled_ExcludedFoundLease()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.2");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.3");

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
                        maskLength: 24,
                        reuseAddressIfPossible: false,
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
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
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAddress, scopeId, rootScope, DateTime.UtcNow);

            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent(1, clientMacAdress, scopeId, rootScope, expectedAddress, lease);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFoundByUniqueIdentifier_NoReuse()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.3");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.1"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.1"),
                            IPv4Address.FromString("192.168.178.2"),
                        },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(
                0, 1, expectedAddress, scopeId, rootScope, DateTime.UtcNow, true, uniqueIdentifier);

            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent
                (1, clientMacAdress, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFoundByUniqueIdentifier_NoReuse_ExcludedFoundLease()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.2");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.3");

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
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        reuseAddressIfPossible: false,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(
                0, 1, expectedAddress, scopeId, rootScope, DateTime.UtcNow, true, uniqueIdentifier);

            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);

            CheckRevokedEvent(0, scopeId, leaseId, rootScope);
            CheckLeaseCreatedEvent
                (1, clientMacAdress, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseFoundByUniqueIdentifier_Reuse()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");

            DateTime leaseCreatedAt = DateTime.UtcNow.AddHours(-1);
            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.1") },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        reuseAddressIfPossible: true,
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = uniqueIdentifier,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            var revokedLease = rootScope.GetScopeById(scopeId).Leases.GetLeaseById(leaseId);

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(leasedAddress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(
                0, 1, leasedAddress, scopeId, rootScope, DateTime.UtcNow, true, uniqueIdentifier);

            Assert.False(revokedLease.IsActive());
            Assert.Equal(LeaseStates.Revoked, revokedLease.State);

            CheckEventAmount(3, rootScope);
            CheckLeaseCreatedEvent
                (1, clientMacAdress, scopeId, rootScope, leasedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(2, discoverPacket, result, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_LeaseNotFoundByUniqueIdentifier_Reuse()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);
            Byte[] uniqueIdentifier = random.NextBytes(20);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(true);
            resolverMock.Setup(x => x.GetUniqueIdentifier(discoverPacket)).Returns(uniqueIdentifier);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            IPv4Address leasedAddress = IPv4Address.FromString("192.168.178.10");
            IPv4Address expectedAddress = IPv4Address.FromString("192.168.178.2");

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
                        reuseAddressIfPossible: true,
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = leaseId,
                    Address = leasedAddress,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = leaseCreatedAt,
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            }); ;

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAddress, result);
            CheckPacketOptions(scopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(
                1, 2, expectedAddress, scopeId, rootScope, DateTime.UtcNow, true, uniqueIdentifier);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent
                (0, clientMacAdress, scopeId, rootScope, expectedAddress, lease, uniqueIdentifier);

            CheckHandeledEvent(1, discoverPacket, result, rootScope, scopeId);
        }

        [Theory]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next)]
        [InlineData(DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Random)]
        public void HandleDiscover_NoLeaseFound_NoAddressAvaiable(DHCPv4ScopeAddressProperties.AddressAllocationStrategies allocationStrategy)
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.0"),
                        IPv4Address.FromString("192.168.178.3"),
                        new List<IPv4Address>{
                            IPv4Address.FromString("192.168.178.0"),
                            IPv4Address.FromString("192.168.178.1"),
                            IPv4Address.FromString("192.168.178.2")},
                        leaseTime: TimeSpan.FromDays(1),
                        maskLength: 24,
                        addressAllocationStrategy: allocationStrategy),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                }),
                new DHCPv4LeaseCreatedEvent
                {
                    EntityId = random.NextGuid(),
                    Address = IPv4Address.FromString("192.168.178.3"),
                    ClientIdenfier = DHCPv4ClientIdentifier.FromHwAddress(random.NextBytes(6)).GetBytes(),
                    ScopeId = scopeId,
                    UniqueIdentifier = null,
                    StartedAt = DateTime.UtcNow.AddDays(-1),
                    ValidUntil = DateTime.UtcNow.AddDays(1),
                 },
            });

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            Assert.Equal(DHCPv4Packet.Empty, result);

            CheckEventAmount(2, rootScope);
            DHCPv4ScopeAddressesAreExhaustedEvent(0, rootScope, scopeId);
            CheckHandeledEventNotSuccessfull(1, discoverPacket, rootScope, scopeId);
        }

        [Fact]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_ExcludedAddressFromSubscopes()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var parentResolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            var childResolverInformation = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> parentResolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            parentResolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            parentResolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> childesolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            childesolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(parentResolverInformations)).Returns(parentResolverMock.Object);
            scopeResolverMock.Setup(x => x.InitializeResolver(childResolverInformation)).Returns(childesolverMock.Object);

            Guid parentScopeId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.10"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.21") },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = parentResolverInformations,
                    Name = "Parent Scope",
                    Id = parentScopeId,
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.10"),
                        IPv4Address.FromString("192.168.178.20"),
                        Array.Empty<IPv4Address>()),
                    ResolverInformation = childResolverInformation,
                    Name = "Child scope",
                    Id = scopeId,
                    ParentId = parentScopeId
                })
            });

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.22");

            DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, result);
            CheckPacketOptions(parentScopeId, rootScope, result);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, parentScopeId, rootScope, DateTime.UtcNow);
        }

        [Fact]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_ExcludedAddressFromSubscopes_Random()
        {
            Random random = new Random();

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            Byte[] clientMacAdress = random.NextBytes(6);

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, (UInt32)random.Next(),
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var parentResolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            var childResolverInformation = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> parentResolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            parentResolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            parentResolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> childesolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            childesolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(parentResolverInformations)).Returns(parentResolverMock.Object);
            scopeResolverMock.Setup(x => x.InitializeResolver(childResolverInformation)).Returns(childesolverMock.Object);

            Guid parentScopeId = random.NextGuid();
            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.10"),
                        IPv4Address.FromString("192.168.178.255"),
                        new List<IPv4Address>{IPv4Address.FromString("192.168.178.21") },
                        leaseTime: TimeSpan.FromDays(1),
                        renewalTime: TimeSpan.FromHours(12),
                        preferredLifetime: TimeSpan.FromHours(20),
                        maskLength: 24,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = parentResolverInformations,
                    Name = "Parent Scope",
                    Id = parentScopeId,
                }),
                new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.10"),
                        IPv4Address.FromString("192.168.178.20"),
                        Array.Empty<IPv4Address>()),
                    ResolverInformation = childResolverInformation,
                    Name = "Child scope",
                    Id = scopeId,
                    ParentId = parentScopeId
                })
            });

            for (int i = 0; i < 150; i++)
            {
                DHCPv4Packet result = rootScope.HandleDiscover(discoverPacket);
                IPv4Address clientAddress = result.YourIPAdress;

                Assert.True(clientAddress >= IPv4Address.FromString("192.168.178.22"));
                Assert.True(clientAddress <= IPv4Address.FromString("192.168.178.255"));
            }
        }

        [Fact]
        public void HandleDiscover_NoLeaseFound_AddressAvaiable_FullCycle_WithRenew()
        {
            Random random = new Random();

            IPv4Address expectedAdress = IPv4Address.FromString("192.168.178.13");

            IPv4HeaderInformation headerInformation =
                new IPv4HeaderInformation(IPv4Address.Empty, IPv4Address.Broadcast);

            IPv4HeaderInformation secondHeaderInformation =
         new IPv4HeaderInformation(expectedAdress, IPv4Address.FromString("192.168.20.0"));

            Byte[] clientMacAdress = random.NextBytes(6);

            UInt32 firstTransactionId = (UInt32)random.Next();
            UInt32 secondTransactionId = (UInt32)random.Next();

            DHCPv4Packet discoverPacket = new DHCPv4Packet(
                headerInformation, clientMacAdress, firstTransactionId,
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Discover)
            );

            DHCPv4Packet requestAsAnswerToOfferPacket = new DHCPv4Packet(headerInformation, clientMacAdress, firstTransactionId,
                IPv4Address.Empty, IPv4Address.Empty, IPv4Address.Empty, DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request), new DHCPv4PacketAddressOption(DHCPv4OptionTypes.RequestedIPAddress, expectedAdress), new DHCPv4PacketAddressOption(DHCPv4OptionTypes.ServerIdentifier, IPv4Address.FromString("192.168.20.0")));

            DHCPv4Packet requestPacket = new DHCPv4Packet(secondHeaderInformation, clientMacAdress, firstTransactionId,
             IPv4Address.Empty, IPv4Address.Empty, expectedAdress, DHCPv4PacketFlags.Unicast,
            new DHCPv4PacketMessageTypeOption(DHCPv4MessagesTypes.Request));

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock =
                new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);

            var resolverInformations = new CreateScopeResolverInformation
            {
                Typename = nameof(DHCPv4RelayAgentSubnetResolver),
            };

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.PacketMeetsCondition(discoverPacket)).Returns(true);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestAsAnswerToOfferPacket)).Returns(true);
            resolverMock.Setup(x => x.PacketMeetsCondition(requestPacket)).Returns(true);

            resolverMock.SetupGet(x => x.HasUniqueIdentifier).Returns(false);

            scopeResolverMock.Setup(x => x.InitializeResolver(resolverInformations)).Returns(resolverMock.Object);

            Guid scopeId = random.NextGuid();

            DHCPv4RootScope rootScope = GetRootScope(scopeResolverMock);
            rootScope.Load(new List<DomainEvent>{ new DHCPv4ScopeEvents.DHCPv4ScopeAddedEvent(
                new DHCPv4ScopeCreateInstruction
                {
                    AddressProperties = new DHCPv4ScopeAddressProperties(
                        IPv4Address.FromString("192.168.178.13"),
                        IPv4Address.FromString("192.168.178.13"),
                        Array.Empty<IPv4Address>(),
                        leaseTime: TimeSpan.FromHours(6),
                        renewalTime: TimeSpan.FromHours(3),
                        preferredLifetime: TimeSpan.FromHours(1),
                        maskLength: 24,
                        reuseAddressIfPossible: true,
                        supportDirectUnicast: true,
                        addressAllocationStrategy: DHCPv4ScopeAddressProperties.AddressAllocationStrategies.Next),
                    ResolverInformation = resolverInformations,
                    Name = "Testscope",
                    Id = scopeId,
                })
            });

            DHCPv4Packet offer = rootScope.HandleDiscover(discoverPacket);
            CheckPacket(expectedAdress, offer);
            CheckPacketOptions(scopeId, rootScope, offer);

            DHCPv4Lease lease = CheckLease(0, 1, expectedAdress, scopeId, rootScope, DateTime.UtcNow, false);

            CheckEventAmount(2, rootScope);
            CheckLeaseCreatedEvent(0, clientMacAdress, scopeId, rootScope, expectedAdress, lease);
            CheckHandeledEvent(1, discoverPacket, offer, rootScope, scopeId);

            DHCPv4Packet ack = rootScope.HandleRequest(requestAsAnswerToOfferPacket);
            Assert.Equal(DHCPv4MessagesTypes.Acknowledge, ack.MessageType);
            Assert.Equal(LeaseStates.Active, lease.State);

            DHCPv4Packet secondAck = rootScope.HandleRequest(requestPacket);
            Assert.Equal(DHCPv4MessagesTypes.Acknowledge, secondAck.MessageType);
            Assert.Equal(LeaseStates.Active, lease.State);
        }
    }
}
