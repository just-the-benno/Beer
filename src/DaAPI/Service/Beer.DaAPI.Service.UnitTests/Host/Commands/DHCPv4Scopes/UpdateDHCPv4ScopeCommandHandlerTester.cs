﻿using Beer.DaAPI.Core.Common;
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
    public class UpdateDHCPv4ScopeCommandHandlerTester
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Handle(Boolean storeResult, Boolean useDynamicTime)
        {
            Random random = new Random();

            Guid scopeId = random.NextGuid();
            Guid leaseId = random.NextGuid();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = start + 100;

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolver<DHCPv4Packet, IPv4Address>> resolverMock = new Mock<IScopeResolver<DHCPv4Packet, IPv4Address>>(MockBehavior.Strict);
            resolverMock.Setup(x => x.GetValues()).Returns(new Dictionary<String, String>());

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.IsResolverInformationValid(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(true).Verifiable();
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
            new DHCPv4LeaseCreatedEvent
            {
                    EntityId = leaseId,
                    Address = start - 100,
                    ScopeId = scopeId,
                    ClientIdenfier = DHCPv4ClientIdentifier.FromDuid(1,new UUIDDUID(random.NextGuid()), random.NextBytes(6)).GetBytes(),
            },
            new DHCPv4LeaseActivatedEvent
            {
                EntityId = leaseId,
                ScopeId = scopeId,
            } });

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(storeResult).Verifiable();

            var command = new UpdateDHCPv4ScopeCommand(scopeId, name, description, null,
               useDynamicTime == false ? new DHCPv4ScopeAddressPropertyReqest
               {
                   Start = start.ToString(),
                   End = end.ToString(),
                   ExcludedAddresses = Array.Empty<String>(),
                   AcceptDecline = random.NextBoolean(),
                   AddressAllocationStrategy = DHCPv4ScopeAddressPropertyReqest.AddressAllocationStrategies.Next,
                   InformsAreAllowd = random.NextBoolean(),
                   ReuseAddressIfPossible = random.NextBoolean(),
                   SupportDirectUnicast = random.NextBoolean(),
                   PreferredLifetime = TimeSpan.FromDays(0.5),
                   RenewalTime = TimeSpan.FromDays(0.25),
                   LeaseTime = TimeSpan.FromDays(1),
                   MaskLength = (Byte)random.Next(10, 29),
               } : new DHCPv4ScopeAddressPropertyReqest
               {
                   Start = start.ToString(),
                   End = end.ToString(),
                   ExcludedAddresses = Array.Empty<String>(),
                   AcceptDecline = random.NextBoolean(),
                   AddressAllocationStrategy = DHCPv4ScopeAddressPropertyReqest.AddressAllocationStrategies.Next,
                   InformsAreAllowd = random.NextBoolean(),
                   ReuseAddressIfPossible = random.NextBoolean(),
                   SupportDirectUnicast = random.NextBoolean(),
                   DynamicRenewTime = new DHCPv4DynamicRenewTimeRequest
                   {
                       Hours = 5,
                       Minutes = 10,
                       MinutesToEndOfLife = 45,
                       MinutesToRebound = 35,
                   },
                   MaskLength = (Byte)random.Next(10, 29),
               }
                ,
                new CreateScopeResolverRequest
                {
                    PropertiesAndValues = new Dictionary<String, String>(),
                    Typename = resolverName,
                },
                new DHCPv4ScopePropertyRequest[] {
                    new DHCPv4AddressListScopePropertyRequest
                    {
                     OptionCode = 24,
                     Type = DHCPv4ScopePropertyType.AddressList,
                     Addresses = random.GetIPv4Addresses().Select(x => x.ToString()).ToArray(),
                    },
                   new DHCPv4AddressScopePropertyRequest
                    {
                     OptionCode = 25,
                     Type = DHCPv4ScopePropertyType.Address,
                     Address = random.GetIPv4Address().ToString()
                    },
                    new DHCPv4BooleanScopePropertyRequest
                    {
                     OptionCode = 26,
                     Type = DHCPv4ScopePropertyType.Boolean,
                     Value = random.NextBoolean()
                    },
                    new DHCPv4NumericScopePropertyRequest
                    {
                     OptionCode = 27,
                     Type = DHCPv4ScopePropertyType.UInt16,
                     NumericType = DHCPv4NumericValueTypes.UInt16,
                     Value = (UInt16)random.NextUInt16()
                    },
                    new DHCPv4TextScopePropertyRequest
                    {
                     OptionCode = 28,
                     Type = DHCPv4ScopePropertyType.Text,
                     Value = random.GetAlphanumericString()
                    },
                    new DHCPv4TimeScopePropertyRequest
                    {
                     OptionCode = 29,
                     Type = DHCPv4ScopePropertyType.Time,
                     Value = TimeSpan.FromSeconds(random.Next(10,20))
                    },
                 new DHCPv4AddressListScopePropertyRequest
                {
                 OptionCode = 64,
                 MarkAsRemovedInInheritance = true,
                }
                }
                );

            var serviceBusMock = new Mock<IServiceBus>(MockBehavior.Strict);
            serviceBusMock.Setup(x => x.Publish(It.Is<NewTriggerHappendMessage>(y => y.Triggers.Count() == 1 &&
            ((PrefixEdgeRouterBindingUpdatedTrigger)y.Triggers.First()).OldBinding.Prefix == IPv4Address.FromString("fc70::0")
            ))).Returns(Task.CompletedTask);
            var handler = new UpdateDHCPv4ScopeCommandHandler(storageMock.Object, serviceBusMock.Object, rootScope,
                Mock.Of<ILogger<UpdateDHCPv4ScopeCommandHandler>>());

            Boolean result = await handler.Handle(command, CancellationToken.None);
            Assert.Equal(storeResult, result);

            var scope = rootScope.GetRootScopes().First();

            Assert.Equal(name, scope.Name);
            Assert.Equal(description, scope.Description);
            Assert.NotNull(scope.Resolver);
            Assert.Equal(start, scope.AddressRelatedProperties.Start);

            Assert.True(scope.Properties.IsMarkedAsRemovedFromInheritance(64));
            Assert.False(scope.Properties.IsMarkedAsRemovedFromInheritance(24));

            if (useDynamicTime == true)
            {
                Assert.True(scope.AddressRelatedProperties.UseDynamicRewnewTime);
                Assert.Equal(5, scope.AddressRelatedProperties.DynamicRenewTime.Hour);
            }
            else
            {
                Assert.False(scope.AddressRelatedProperties.UseDynamicRewnewTime);
            }

            scopeResolverMock.Verify();
            storageMock.Verify();

            serviceBusMock.Verify();
            Assert.Empty(rootScope.GetTriggers());

        }
    }
}
