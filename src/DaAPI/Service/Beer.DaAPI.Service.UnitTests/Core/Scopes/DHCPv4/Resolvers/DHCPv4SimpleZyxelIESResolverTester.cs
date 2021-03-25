using Beer.DaAPI.Core.Common;
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
    public class DHCPv4SimpleZyxelIESResolverTester
    {
        [Fact]
        public void DHCPv4SimpleZyxelIESResolver_GetDescription()
        {
            DHCPv4SimpleZyxelIESResolver resolver = new DHCPv4SimpleZyxelIESResolver();

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DHCPv4SimpleZyxelIESResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Equal(3, description.Properties.Count());

            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.First();

                Assert.Equal(nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Byte, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(1);

                Assert.Equal(nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Byte, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(2);

                Assert.Equal(nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.ByteArray, propertyDescription.PropertyValueType);
            }
        }

        [Fact]
        public void DHCPv4SimpleZyxelIESResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();
            String emptyListValue = random.GetAlphanumericString(30);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DHCPv4SimpleZyxelIESResolver resolver = new DHCPv4SimpleZyxelIESResolver();

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
                    //{ nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress), "f323abf23aa" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress), "f323abf23aa" },
                    //{ nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress), "f323abf23aa" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    //{ nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4SimpleZyxelIESResolver_AreValuesValid_InvalidDeviceMacAddress()
        {
            Random random = new Random();
            String emptyListValue = random.GetAlphanumericString(30);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DHCPv4SimpleZyxelIESResolver resolver = new DHCPv4SimpleZyxelIESResolver();

            List<Dictionary<String, String>> invalidInputs = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress), "f323abf23a" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress), "f323abf2qaa" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress), "" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4SimpleZyxelIESResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            Int32 portNumber = random.Next(1, 40);
            Int32 linecardNumber = random.Next(1, 8);
            String macAddressAsString = "f323abf23aa2";

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            mock.Setup(x => x.Deserialze<Byte>(linecardNumber.ToString())).Returns((Byte)linecardNumber).Verifiable();
            mock.Setup(x => x.Deserialze<String>(macAddressAsString)).Returns(macAddressAsString).Verifiable();

            DHCPv4SimpleZyxelIESResolver resolver = new();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress),  macAddressAsString },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), portNumber.ToString() },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), linecardNumber.ToString() },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, mock.Object);
            Assert.True(result);
        }

        [Fact]
        public void DHCPv4SimpleZyxelIESResolver_ApplyValues()
        {
            Random random = new Random();

            Int32 portNumber = 21;
            Int32 linecardNumber = 3;
            String macAddressAsString = "f323a4f23aa2";

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            mock.Setup(x => x.Deserialze<Byte>(linecardNumber.ToString())).Returns((Byte)linecardNumber).Verifiable();
            mock.Setup(x => x.Deserialze<String>(macAddressAsString)).Returns(macAddressAsString).Verifiable();

            DHCPv4SimpleZyxelIESResolver resolver = new();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress),  macAddressAsString },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), portNumber.ToString() },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), linecardNumber.ToString() },
                };

            resolver.ApplyValues(input, mock.Object);
            Assert.Equal(portNumber, resolver.PortNumber);
            Assert.Equal(linecardNumber, resolver.LinecardNumber);
            Assert.Equal(new Byte[] { 0xf3, 0x23, 0xa4, 0xf2, 0x3a, 0xa2 }, resolver.DeviceMacAddress);

            Assert.Equal(new Byte[] { 0x01, 0x04, 0x33, 0x2f, 0x32, 0x31, 0x02, 0x06, 0xf3, 0x23, 0xa4, 0xf2, 0x3a, 0xa2 }, resolver.Value);

            mock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DHCPv4SimpleZyxelIESResolver_PacketMeetsConditions(Boolean shouldPass)
        {
            Random random = new Random();

            Int32 portNumber = 21;
            Int32 linecardNumber = 3;
            String macAddressAsString = "f323a4f23a";

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            mock.Setup(x => x.Deserialze<Byte>(linecardNumber.ToString())).Returns((Byte)linecardNumber).Verifiable();
            mock.Setup(x => x.Deserialze<String>(macAddressAsString)).Returns(macAddressAsString).Verifiable();

            DHCPv4SimpleZyxelIESResolver resolver = new();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleZyxelIESResolver.DeviceMacAddress),  macAddressAsString },
                    { nameof(DHCPv4SimpleZyxelIESResolver.PortNumber), portNumber.ToString() },
                    { nameof(DHCPv4SimpleZyxelIESResolver.LinecardNumber), linecardNumber.ToString() },
                };

            resolver.ApplyValues(input, mock.Object);

            Byte[] option82InsidePacket = ByteHelper.CopyData(resolver.Value);
            if (shouldPass == false)
            {
                option82InsidePacket[^1]--;
            }

            DHCPv4Packet packet = new DHCPv4Packet(
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                random.NextBytes(6),
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty,
                new DHCPv4PacketRawByteOption(82, option82InsidePacket)
                );

            Boolean actual = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldPass, actual);
        }
    }
}
