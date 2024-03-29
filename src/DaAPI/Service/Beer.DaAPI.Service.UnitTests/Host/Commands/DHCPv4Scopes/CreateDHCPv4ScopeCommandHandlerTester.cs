﻿using Beer.DaAPI.Core.Common;
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
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using Beer.DaAPI.Service.TestHelper;
using Beer.DaAPI.Infrastructure.ServiceBus;

namespace Beer.DaAPI.UnitTests.Host.Commands.DHCPv4Scopes
{
    public class CreateDHCPv4ScopeCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean withDynamicRenewTime)
        {
            Random random = new Random();

            String name = random.GetAlphanumericString();
            String description = random.GetAlphanumericString();

            IPv4Address start = random.GetIPv4Address();
            IPv4Address end = start + 100;

            String resolverName = random.GetAlphanumericString();

            Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>> scopeResolverMock = new Mock<IScopeResolverManager<DHCPv4Packet, IPv4Address>>();
            scopeResolverMock.Setup(x => x.IsResolverInformationValid(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(true).Verifiable();
            scopeResolverMock.Setup(x => x.InitializeResolver(It.Is<CreateScopeResolverInformation>(y =>
            y.Typename == resolverName
            ))).Returns(Mock.Of<IScopeResolver<DHCPv4Packet, IPv4Address>>()).Verifiable();

            Mock<ILoggerFactory> factoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            factoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(Mock.Of<ILogger<DHCPv4RootScope>>());

            DHCPv4RootScope rootScope = new DHCPv4RootScope(random.NextGuid(), scopeResolverMock.Object, factoryMock.Object);

            Mock<IDHCPv4StorageEngine> storageMock = new Mock<IDHCPv4StorageEngine>(MockBehavior.Strict);
            storageMock.Setup(x => x.Save(rootScope)).ReturnsAsync(true).Verifiable();

            var command = new CreateDHCPv4ScopeCommand(name, description, null,
                withDynamicRenewTime == false ? new DHCPv4ScopeAddressPropertyReqest
                {
                    Start = start.ToString(),
                    End = end.ToString(),
                    ExcludedAddresses = Array.Empty<String>(),
                    AcceptDecline = random.NextBoolean(),
                    AddressAllocationStrategy = DHCPv4ScopeAddressPropertyReqest.AddressAllocationStrategies.Next,
                    InformsAreAllowd = random.NextBoolean(),
                    ReuseAddressIfPossible = random.NextBoolean(),
                    SupportDirectUnicast = random.NextBoolean(),
                    RenewalTime = TimeSpan.FromDays(0.5),
                    PreferredLifetime = TimeSpan.FromDays(0.8),
                    LeaseTime = TimeSpan.FromDays(1),
                    MaskLength = (Byte)random.Next(12, 20),
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
                    MaskLength = (Byte)random.Next(12, 20),
                    DynamicRenewTime = new DHCPv4DynamicRenewTimeRequest
                    {
                        Hours = 3,
                        Minutes = 15,
                        MinutesToEndOfLife = 40,
                        MinutesToRebound = 20,
                    }
                },
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

            var handler = new CreateDHCPv4ScopeCommandHandler(storageMock.Object, rootScope, Mock.Of<IServiceBus>(MockBehavior.Strict),
                Mock.Of<ILogger<CreateDHCPv4ScopeCommandHandler>>());

            Guid? result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result.HasValue);

            var scope = rootScope.GetRootScopes().First();
            Assert.True(scope.Properties.IsMarkedAsRemovedFromInheritance(64));
            Assert.False(scope.Properties.IsMarkedAsRemovedFromInheritance(24));

            if(withDynamicRenewTime == false)
            {
                Assert.False(scope.AddressRelatedProperties.UseDynamicRewnewTime);
            }
            else
            {
                Assert.True(scope.AddressRelatedProperties.UseDynamicRewnewTime);
                Assert.Equal(3, scope.AddressRelatedProperties.DynamicRenewTime.Hour);
            }

            scopeResolverMock.Verify();
            storageMock.Verify();
        }
    }
}
