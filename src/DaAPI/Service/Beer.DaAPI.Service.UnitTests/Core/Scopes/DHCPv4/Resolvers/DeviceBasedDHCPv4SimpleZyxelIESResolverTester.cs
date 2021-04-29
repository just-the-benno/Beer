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
    public class DeviceBasedDHCPv4SimpleZyxelIESResolverTester
    {
        [Fact]
        public void DeviceBasedDHCPv4SimpleZyxelIESResolver_GetDescription()
        {
            var resolver = new DeviceBasedDHCPv4SimpleZyxelIESResolver(Mock.Of<IDeviceService>());

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Equal(3, description.Properties.Count());

            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.First();

                Assert.Equal(nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Byte, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(1);

                Assert.Equal(nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Byte, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(2);

                Assert.Equal(nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Device, propertyDescription.PropertyValueType);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleZyxelIESResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();
            String emptyListValue = random.GetAlphanumericString(30);

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DeviceBasedDHCPv4SimpleZyxelIESResolver resolver = new DeviceBasedDHCPv4SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

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
                    //{ nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId), "91c1a3bf-94e3-4168-a260-bdb44f109355" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId), "91c1a3bf-94e3-4168-a260-bdb44f109355" },
                    //{ nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId), "91c1a3bf-94e3-4168-a260-bdb44f109355" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    //{ nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleZyxelIESResolver_AreValuesValid_InvalidDeviceMacAddress()
        {
            Random random = new Random();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            DeviceBasedDHCPv4SimpleZyxelIESResolver resolver = new DeviceBasedDHCPv4SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            List<Dictionary<String, String>> invalidInputs = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId), "" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                 new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId), null },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
                  new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId), "91c1a3bf-94e3-4168-a260-bdb44f10935" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), "2" },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), "1" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleZyxelIESResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            Int32 portNumber = random.Next(1, 40);
            Int32 linecardNumber = random.Next(1, 8);
            Guid deviceId = random.NextGuid();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);
            mock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            mock.Setup(x => x.Deserialze<Byte>(linecardNumber.ToString())).Returns((Byte)linecardNumber).Verifiable();
            mock.Setup(x => x.Deserialze<String>(deviceId.ToString())).Returns(deviceId.ToString()).Verifiable();

            DeviceBasedDHCPv4SimpleZyxelIESResolver resolver = new(Mock.Of<IDeviceService>(MockBehavior.Strict));

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), portNumber.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), linecardNumber.ToString() },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, mock.Object);
            Assert.True(result);
        }

        [Fact]
        public void DeviceBasedDHCPv4SimpleZyxelIESResolver_ApplyValues()
        {
            Random random = new Random();

            Int32 portNumber = 21;
            Int32 linecardNumber = 3;

            Guid deviceId = random.NextGuid();

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Byte>(linecardNumber.ToString())).Returns((Byte)linecardNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(new byte[] { 0xf3, 0x23, 0xa4, 0xf2, 0x3a, 0xa2 });

            DeviceBasedDHCPv4SimpleZyxelIESResolver resolver = new(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), portNumber.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), linecardNumber.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);
            Assert.Equal(portNumber, resolver.PortNumber);
            Assert.Equal(linecardNumber, resolver.LinecardNumber);
            Assert.Equal(deviceId, resolver.DeviceId);

            Assert.NotNull(resolver.ValueGetter);
            Assert.Null(resolver.Value);
            var value = resolver.ValueGetter();


            Assert.Equal(new Byte[] { 0x01, 0x04, 0x33, 0x2f, 0x32, 0x31, 0x02, 0x06, 0xf3, 0x23, 0xa4, 0xf2, 0x3a, 0xa2 }, value);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeviceBasedDHCPv4SimpleZyxelIESResolver_PacketMeetsConditions(Boolean shouldPass)
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();

            Int32 portNumber = 21;
            Int32 linecardNumber = 3;

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Byte>(portNumber.ToString())).Returns((Byte)portNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Byte>(linecardNumber.ToString())).Returns((Byte)linecardNumber).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(new byte[] { 0xf3, 0x23, 0xa4, 0xf2, 0x3a, 0xa2 });

            DeviceBasedDHCPv4SimpleZyxelIESResolver resolver = new(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.PortNumber), portNumber.ToString() },
                    { nameof(DeviceBasedDHCPv4SimpleZyxelIESResolver.LinecardNumber), linecardNumber.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);

            Byte[] option82InsidePacket = ByteHelper.CopyData(resolver.ValueGetter());
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
