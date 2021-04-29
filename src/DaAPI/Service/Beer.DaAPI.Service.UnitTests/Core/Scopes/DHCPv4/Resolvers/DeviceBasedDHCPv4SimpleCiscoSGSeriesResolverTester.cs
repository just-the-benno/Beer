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
    public class DeviceBasedDHCPv4SimpleCiscoSGSeriesResolverTester
    {
        [Fact]
        public void DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver_GetDescription()
        {
            DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver resolver = new DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Equal(3, description.Properties.Count());

            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.First();

                Assert.Equal(nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.VLANId, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(1);

                Assert.Equal(nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Byte, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(2);

                Assert.Equal(nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Device, propertyDescription.PropertyValueType);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver resolver = new DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

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
                    //{ nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId), "ea6a012a-04d9-474c-9ff5-2d9eff61b710" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId), "ea6a012a-04d9-474c-9ff5-2d9eff61b710" },
                    //{ nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId), "ea6a012a-04d9-474c-9ff5-2d9eff61b710" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    //{ nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver_AreValuesValid_InvalidDeviceId()
        {
            Random random = new Random();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver resolver = new DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            List<Dictionary<String, String>> invalidInputs = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId), "" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId), null },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId), "eb2d5f55-414e-4ff0-86c6-b886c70aae6" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            Int32 portNumber = random.Next(1, 40);
            Int32 vlanNumber = random.Next(1, 4094);
            Guid deviceId = random.NextGuid();

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(vlanNumber.ToString())).Returns((UInt16)vlanNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(deviceId.ToString())).Returns(deviceId.ToString()).Verifiable();

            DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver resolver = new(Mock.Of<IDeviceService>(MockBehavior.Strict));

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), portNumber.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), vlanNumber.ToString() },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, serializerMock.Object);
            Assert.True(result);

            serializerMock.Verify();
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver_ApplyValues()
        {
            Random random = new Random();

            Int32 portNumber = 22;
            Int32 vlanNumber = 97;
            Guid deviceId = random.NextGuid();

            String macAddressAsString = "00b1e3da247f";

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(vlanNumber.ToString())).Returns((Byte)vlanNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f });

            DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver resolver = new(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), portNumber.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), vlanNumber.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);
            Assert.Equal(portNumber, resolver.PortNumber);
            Assert.Equal(vlanNumber, resolver.VlanNumber);
            Assert.Equal(deviceId, resolver.DeviceId);

            Assert.Null(resolver.Value);

            Assert.Equal(new Byte[] { 0x01, 0x06, 0x00, 0x04, 0x00, 0x61, 0x01, 0x16, 0x02, 0x08, 0x00, 0x06, 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f }, resolver.ValueGetter());

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver_PacketMeetsConditions(Boolean shouldPass)
        {
            Random random = new Random();

            Int32 portNumber = 21;
            Int32 vlanNumber = 3;
            Guid deviceId = random.NextGuid();
            String macAddressAsString = "f323a4f23a";

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(vlanNumber.ToString())).Returns((Byte)vlanNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(new Byte[] { 0xf3, 0x23, 0xa4, 0xf2, 0x3a });

            DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver resolver = new(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.PortNumber), portNumber.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver.VlanNumber), vlanNumber.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);

            Byte[] option82InsidePacket = ByteHelper.CopyData(resolver.ValueGetter());
            if (shouldPass == false)
            {
                option82InsidePacket[^1]--;
            }

            DHCPv4Packet packet = new(
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

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }
    }
}
