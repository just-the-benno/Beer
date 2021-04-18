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
using static Beer.DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public class DHCPv4SimpleCiscoSGSeriesResolverTester
    {
        [Fact]
        public void DHCPv4SimpleCiscoSGSeriesResolver_GetDescription()
        {
            DHCPv4SimpleCiscoSGSeriesResolver resolver = new DHCPv4SimpleCiscoSGSeriesResolver();

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DHCPv4SimpleCiscoSGSeriesResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Equal(3, description.Properties.Count());

            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.First();

                Assert.Equal(nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.VLANId, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(1);

                Assert.Equal(nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Byte, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(2);

                Assert.Equal(nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.ByteArray, propertyDescription.PropertyValueType);
            }
        }

        [Fact]
        public void DHCPv4SimpleCiscoSGSeriesResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DHCPv4SimpleCiscoSGSeriesResolver resolver = new DHCPv4SimpleCiscoSGSeriesResolver();

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
                    //{ nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress), "f323abf23aa" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress), "f323abf23aa" },
                    //{ nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress), "f323abf23aa" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    //{ nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4SimpleCiscoSGSeriesResolver_AreValuesValid_InvalidDeviceMacAddress()
        {
            Random random = new Random();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DHCPv4SimpleCiscoSGSeriesResolver resolver = new DHCPv4SimpleCiscoSGSeriesResolver();

            List<Dictionary<String, String>> invalidInputs = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress), "f323abf23a" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress), "f323abf2qaa" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress), "" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4SimpleCiscoSGSeriesResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            Int32 portNumber = random.Next(1, 40);
            Int32 vlanNumber = random.Next(1, 4094);
            String macAddressAsString = "f323abf23aa2";

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            mock.Setup(x => x.Deserialze<UInt16>(vlanNumber.ToString())).Returns((UInt16)vlanNumber).Verifiable();
            mock.Setup(x => x.Deserialze<String>(macAddressAsString)).Returns(macAddressAsString).Verifiable();

            DHCPv4SimpleCiscoSGSeriesResolver resolver = new();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress),  macAddressAsString },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), portNumber.ToString() },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), vlanNumber.ToString() },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, mock.Object);
            Assert.True(result);
        }

        [Fact]
        public void DHCPv4SimpleCiscoSGSeriesResolver_ApplyValues()
        {
            Random random = new Random();

            Int32 portNumber = 22;
            Int32 vlanNumber = 97;
            String macAddressAsString = "00b1e3da247f";

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            mock.Setup(x => x.Deserialze<UInt16>(vlanNumber.ToString())).Returns((Byte)vlanNumber).Verifiable();
            mock.Setup(x => x.Deserialze<String>(macAddressAsString)).Returns(macAddressAsString).Verifiable();

            DHCPv4SimpleCiscoSGSeriesResolver resolver = new();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress),  macAddressAsString },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), portNumber.ToString() },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), vlanNumber.ToString() },
                };

            resolver.ApplyValues(input, mock.Object);
            Assert.Equal(portNumber, resolver.PortNumber);
            Assert.Equal(vlanNumber, resolver.VlanNumber);
            Assert.Equal(new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f }, resolver.DeviceMacAddress);

            Assert.Equal(new Byte[] { 0x01, 0x06, 0x00, 0x04, 0x00, 0x61, 0x01, 0x16, 0x02, 0x08, 0x00, 0x06, 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f }, resolver.Value);

            mock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DHCPv4SimpleCiscoSGSeriesResolver_PacketMeetsConditions(Boolean shouldPass)
        {
            Random random = new Random();

            Int32 portNumber = 21;
            Int32 vlanNumber = 3;
            String macAddressAsString = "f323a4f23a";

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            mock.Setup(x => x.Deserialze<UInt16>(vlanNumber.ToString())).Returns((Byte)vlanNumber).Verifiable();
            mock.Setup(x => x.Deserialze<String>(macAddressAsString)).Returns(macAddressAsString).Verifiable();

            DHCPv4SimpleCiscoSGSeriesResolver resolver = new();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.DeviceMacAddress),  macAddressAsString },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.PortNumber), portNumber.ToString() },
                    { nameof(DHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), vlanNumber.ToString() },
                };

            resolver.ApplyValues(input, mock.Object);

            Byte[] option82InsidePacket = ByteHelper.CopyData(resolver.Value);
            if (shouldPass == false)
            {
                option82InsidePacket[^1]--;
            }

            DHCPv4Packet packet = new (
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                random.NextBytes(6),
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketRawByteOption(82, option82InsidePacket)
                );

            Boolean actual = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldPass, actual);
        }
    }
}
