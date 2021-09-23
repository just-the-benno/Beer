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
    public class DeviceBasedDHCPv4MacAddressResolverTester
    {
        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_GetDescription()
        {
            var resolver = new DeviceBasedDHCPv4MacAddressResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DeviceBasedDHCPv4MacAddressResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Equal(2, description.Properties.Count());


            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.First();

                Assert.Equal(nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Device, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(1);

                Assert.Equal(nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Boolean, propertyDescription.PropertyValueType);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

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
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId), "ea6a012a-04d9-474c-9ff5-2d9eff61b710" },
                    //{ nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), "true" },
                },
                 new Dictionary<string, string>()
                {
                    //{ nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId), "ea6a012a-04d9-474c-9ff5-2d9eff61b710" },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), "true" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            Boolean searchClientIdenfifier = random.NextBoolean();
            Guid deviceId = random.NextGuid();

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, serializerMock.Object);
            Assert.True(result);

            serializerMock.Verify();
        }

        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_ApplyValues()
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();
            Boolean searchClientIdenfifier = random.NextBoolean();

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f });

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);
            Assert.Equal(searchClientIdenfifier, resolver.SearchClientIdenfifier);
            Assert.Equal(deviceId, resolver.DeviceId);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeviceBasedDHCPv4MacAddressResolver_PacketMeetsConditions_MacAddressInHeader(Boolean shouldPass)
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();
            Boolean searchClientIdenfifier = random.NextBoolean();

            Byte[] resolverMacAddress = new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f };

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(resolverMacAddress);

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);


            Byte[] resolverMacAddress2 = ByteHelper.CopyData(resolverMacAddress);
            if (shouldPass == false)
            {
                resolverMacAddress2[^1]--;
            }

            DHCPv4Packet packet = new(
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                resolverMacAddress2,
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast
                );

            Boolean actual = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldPass, actual);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeviceBasedDHCPv4MacAddressResolver_MacAddressNotFound_NoClientIdentifier(Boolean searchClientIdenfifier)
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();

            Byte[] resolverMacAddress = new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f };

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(resolverMacAddress);

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);


            Byte[] resolverMacAddress2 = ByteHelper.CopyData(resolverMacAddress);
            resolverMacAddress2[^1]--;

            DHCPv4Packet packet = new(
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                resolverMacAddress2,
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast
                );

            Boolean actual = resolver.PacketMeetsCondition(packet);
            Assert.False(actual);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_MacAddressNotFound_ClientIdentifierNoMacAddress()
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();
            Boolean searchClientIdenfifier = true;

            Byte[] resolverMacAddress = new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f };

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(resolverMacAddress);

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);

            Byte[] resolverMacAddress2 = ByteHelper.CopyData(resolverMacAddress);
            resolverMacAddress2[^1]--;

            DHCPv4Packet packet = new(
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                resolverMacAddress2,
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromIdentifierValue("some string"))
                );

            Boolean actual = resolver.PacketMeetsCondition(packet);
            Assert.False(actual);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_MacAddressNotFound_ClientIdentifierWithMacAddress()
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();
            Boolean searchClientIdenfifier = true;

            Byte[] resolverMacAddress = new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f };

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(resolverMacAddress);

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);

            Byte[] resolverMacAddress2 = ByteHelper.CopyData(resolverMacAddress);
            resolverMacAddress2[^1]--;

            DHCPv4Packet packet = new(
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                resolverMacAddress2,
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast,
                new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromHwAddress(resolverMacAddress))
                );

            Boolean actual = resolver.PacketMeetsCondition(packet);
            Assert.True(actual);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_MacAddressNotFound_ClientIdentifierWithDUID()
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();
            Boolean searchClientIdenfifier = true;

            Byte[] resolverMacAddress = new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f };

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(resolverMacAddress);

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);

            Byte[] resolverMacAddress2 = ByteHelper.CopyData(resolverMacAddress);
            resolverMacAddress2[^1]--;

            Dictionary<DUID, Boolean> duids = new Dictionary<DUID, bool>
            {
                { new UUIDDUID(random.NextGuid()), false },
                { new LinkLayerAddressDUID(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,resolverMacAddress), true },
                {  LinkLayerAddressAndTimeDUID.FromEthernet(resolverMacAddress,DateTime.Now), true },
                { new LinkLayerAddressDUID(LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,resolverMacAddress2), false },
                {  LinkLayerAddressAndTimeDUID.FromEthernet(resolverMacAddress2,DateTime.Now), false },
            };

            foreach (var item in duids)
            {
                DHCPv4Packet packet = new(
                     new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                     resolverMacAddress2,
                     (UInt32)random.Next(),
                     IPv4Address.Empty,
                     IPv4Address.Empty,
                     IPv4Address.Empty,
                     DHCPv4PacketFlags.Unicast,
                     new DHCPv4PacketClientIdentifierOption(DHCPv4ClientIdentifier.FromDuid(random.NextUInt16(), item.Key))
                     );

                Boolean actual = resolver.PacketMeetsCondition(packet);
                Assert.Equal(item.Value, actual);
            }

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Fact]
        public void DeviceBasedDHCPv4MacAddressResolver_GetValues()
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();
            Boolean searchClientIdenfifier = random.NextBoolean();

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f });

            var resolver = new DeviceBasedDHCPv4MacAddressResolver(deviceServiceMock.Object);

            var input = new Dictionary<string, string>()
                {
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.DeviceId),  deviceId.ToString() },
                    { nameof(DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);
            Assert.Equal(searchClientIdenfifier, resolver.SearchClientIdenfifier);
            Assert.Equal(deviceId, resolver.DeviceId);

            var values = resolver.GetValues();
            Assert.Equal(2, values.Count);
            Assert.Equal(new Dictionary<String, String>
            {
                { nameof( DeviceBasedDHCPv4MacAddressResolver.DeviceId), deviceId.ToString() },
                { nameof( DeviceBasedDHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier == false ? "false" : "true" },

            }, values, new NonStrictDictionaryComparer<String, String>());

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }
    }
}
