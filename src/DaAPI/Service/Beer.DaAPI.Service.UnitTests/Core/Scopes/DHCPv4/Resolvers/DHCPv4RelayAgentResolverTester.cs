﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public class DHCPv4RelayAgentResolverTester
    {
        [Fact]
        public void DHCPv4RelayAgentResolver_GetDescription()
        {
            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver();

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DHCPv4RelayAgentResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Single(description.Properties);

            ScopeResolverPropertyDescription propertyDescription = description.Properties.First();
            Assert.Equal(nameof(DHCPv4RelayAgentResolver.AgentAddresses), propertyDescription.PropertyName);
            Assert.Equal(ScopeResolverPropertyValueTypes.IPv4AddressList, propertyDescription.PropertyValueType);
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();
            String emptyListValue = random.GetAlphanumericString(30);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<IEnumerable<IPv4Address>>(emptyListValue)).Returns(new List<IPv4Address>());

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver();

            List<Dictionary<String, String>> invalidInputs = new List<Dictionary<string, string>>
            {
                null,
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
                {
                    { random.GetAlphanumericString(10), random.GetAlphanumericString(10)   }
                },
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses), "" },
                },
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses), emptyListValue },
                }
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            List<IPv4Address> agentAddresses = random.GetIPv4Addresses();
            String serializedValues = System.Text.Json.JsonSerializer.Serialize(agentAddresses.Select(x => x.ToString()));

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<IEnumerable<String>>(serializedValues)).Returns(agentAddresses.Select(x => x.ToString()));

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses), serializedValues },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, mock.Object);
            Assert.True(result);
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_ApplyValues()
        {
            Random random = new Random();
            List<IPv4Address> agentAddresses = random.GetIPv4Addresses();
            String serializedValues = System.Text.Json.JsonSerializer.Serialize(agentAddresses.Select(x => x.ToString()));

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<IEnumerable<String>>(serializedValues)).Returns(agentAddresses.Select(x => x.ToString()));

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4RelayAgentResolver.AgentAddresses),serializedValues  },
                };

            resolver.ApplyValues(input, mock.Object);
            Assert.Equal(agentAddresses, resolver.AgentAddresses);
        }

        [Fact]
        public void DHCPv4RelayAgentResolver_PacketMeetsConditions()
        {
            Random random = new Random();
            List<IPv4Address> addresses = random.GetIPv4Addresses();

            String serializedValues = System.Text.Json.JsonSerializer.Serialize(addresses.Select(x => x.ToString()));

            Mock<ISerializer> serializer = new Mock<ISerializer>(MockBehavior.Strict);
            serializer.Setup(x => x.Deserialze<IEnumerable<String>>(serializedValues)).Returns(addresses.Select(x => x.ToString()));

            DHCPv4RelayAgentResolver resolver = new DHCPv4RelayAgentResolver();
            Dictionary<String, String> values = new Dictionary<String, String>() { { nameof(DHCPv4RelayAgentResolver.AgentAddresses), serializedValues } };
            resolver.ApplyValues(values, serializer.Object);

            foreach (IPv4Address item in addresses)
            {
                Boolean shouldPass = false;

                IPv4Address gwAddress = IPv4Address.Empty;
                if (random.NextDouble() > 0.5)
                {
                    gwAddress = item;
                    shouldPass = true;
                }
                else
                {
                    if (random.NextDouble() > 0.5)
                    {
                        gwAddress = random.GetIPv4Address();
                    }
                }

                DHCPv4Packet packet = new DHCPv4Packet(
                  new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                  random.NextBytes(6),
                  (UInt32)random.Next(),
                  IPv4Address.Empty,
                  gwAddress,
                  IPv4Address.Empty
                  );

                Boolean actual = resolver.PacketMeetsCondition(packet);
                Assert.Equal(shouldPass, actual);
            }
        }
    }
}
