using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv6.Resolvers
{
    public class DeviceBasedDHCPv6ClientDUIDResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DeviceBasedDHCPv6ClientDUIDResolver(Mock.Of<IDeviceService>());
            Assert.True(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            LinkLayerAddressDUID duid = new LinkLayerAddressDUID(
    LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,
    new Byte[] { 0x5c, 0xa6, 0x2d, 0xd9, 0x88, 0x00 });

            Random random = new Random();

            var packet = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
               true, 1, random.GetIPv6Address(), random.GetIPv6Address(), Array.Empty<DHCPv6PacketOption>(), DHCPv6RelayPacket.AsInnerRelay(
            true, 0, IPv6Address.FromString("2004::1"), IPv6Address.FromString("fe80::1"), new DHCPv6PacketOption[]
           {
           }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, new[] { new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, duid) })));

            var resolver = new DeviceBasedDHCPv6ClientDUIDResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            Assert.Equal(duid.GetAsByteStream(), resolver.GetUniqueIdentifier(packet));
        }

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("something", false)]
        [InlineData("1e8f2bf9-a7c1-4bb6-8e7c-90947a1fe70a", true)]
        [InlineData("1e8f2bf9-a7c1-4bb6-8e7c-90947a1fe70", false)]
        public void ArePropertiesAndValuesValid(String deviceId, Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(deviceId)).Returns(deviceId);

            var resolver = new DeviceBasedDHCPv6ClientDUIDResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "DeviceId", deviceId },
            }, serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);

            serializerMock.Verify();
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{ { "DeviceId2", "someVaue" } },
                };

            var resolver = new DeviceBasedDHCPv6ClientDUIDResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            foreach (var item in input)
            {
                Boolean actual = resolver.ArePropertiesAndValuesValid(item, Mock.Of<ISerializer>(MockBehavior.Strict));
                Assert.False(actual);
            }
        }

        [Fact]
        public void ApplyValues()
        {
            Random rand = new Random();

            Guid deviceId = rand.NextGuid();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var resolver = new DeviceBasedDHCPv6ClientDUIDResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            resolver.ApplyValues(new Dictionary<String, String> {
               { "DeviceId", deviceId.ToString() },
            }, serializerMock.Object);

            Assert.Equal(deviceId, resolver.DeviceId);
            serializerMock.Verify();

            var values = resolver.GetValues();

            Dictionary<String, String> expectedValues = new Dictionary<string, string>
            {
                 { "DeviceId", deviceId.ToString() }
            };

            Assert.Equal(expectedValues.ToArray(), values.ToArray());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();

            LinkLayerAddressDUID duid = new LinkLayerAddressDUID(
                LinkLayerAddressDUID.DUIDLinkLayerTypes.Ethernet,
                new Byte[] { 0x5c, 0xa6, 0x2d, 0xd9, 0x88, 0x00 });

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            Mock<IDeviceService> deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetDuidFromDevice(deviceId)).Returns(duid).Verifiable();

            var resolver = new DeviceBasedDHCPv6ClientDUIDResolver(deviceServiceMock.Object);
            resolver.ApplyValues(new Dictionary<String, String> {
               { "DeviceId", deviceId.ToString() },
            }, serializerMock.Object);

            var packet = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                true, 1, random.GetIPv6Address(), random.GetIPv6Address(), Array.Empty<DHCPv6PacketOption>(), DHCPv6RelayPacket.AsInnerRelay(
             true, 0, IPv6Address.FromString("2004::1"), IPv6Address.FromString("fe80::1"), new DHCPv6PacketOption[]
            {
            }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, new[] { new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, shouldMeetCondition == true ? (DUID)duid : new UUIDDUID(random.NextGuid())) })));

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DeviceBasedDHCPv6ClientDUIDResolver", new[] {
            new ScopeResolverPropertyDescription("DeviceId",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Device),
            });

            var resolver = new DeviceBasedDHCPv6ClientDUIDResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }
    }
}
