using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6.Resolvers;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Infrastructure.Services.JsonConverters;
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
    public class DeviceBasedDHCPv6PeerAddressResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DeviceBasedDHCPv6PeerAddressResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            Assert.True(resolver.HasUniqueIdentifier);
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            Random random = new Random();

            IPv6Address address = IPv6Address.FromString("fced::1");

            Guid deviceId = random.NextGuid();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            Mock<IDeviceService> deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetIPv6LinkLocalAddressFromDevice(deviceId)).Returns(address);

            var resolver = new DeviceBasedDHCPv6PeerAddressResolver(deviceServiceMock.Object);
            resolver.ApplyValues(new Dictionary<String, String> {
               { "DeviceId", deviceId.ToString() },

            }, serializerMock.Object);

            Assert.Equal(address.GetBytes(), resolver.GetUniqueIdentifier(null));

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Theory]
        [InlineData("",false)]
        [InlineData(null, false)]
        [InlineData("cb6277d0-b846-4351-af9d-bd40375d155a", true)]
        [InlineData("cb6277d0-b846-4351-af9d-bd40375d155", false)]
        public void ArePropertiesAndValuesValid(String deviceId,  Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<String>(deviceId)).Returns(deviceId).Verifiable();

            var resolver = new DeviceBasedDHCPv6PeerAddressResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
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
                new  Dictionary<String,String>{ 
                    { "DeviceId2", "someVaue" },
                },
                };

            var resolver = new DeviceBasedDHCPv6PeerAddressResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            foreach (var item in input)
            {
                Boolean actual = resolver.ArePropertiesAndValuesValid(item, Mock.Of<ISerializer>(MockBehavior.Strict));
                Assert.False(actual);
            }
        }

        [Fact]
        public void ApplyValues()
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var resolver = new DeviceBasedDHCPv6PeerAddressResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            resolver.ApplyValues(new Dictionary<String, String> {
               { "DeviceId", deviceId.ToString() },

            }, serializerMock.Object);

            Assert.Equal(deviceId, resolver.DeviceId);

            serializerMock.Verify();

            Dictionary<String, String> expectedValues = new Dictionary<String, String> {
               { "DeviceId", deviceId.ToString()  },
            };

            Assert.Equal(expectedValues.ToArray(), resolver.GetValues().ToArray());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            Guid deviceId = random.NextGuid();

            String ipAddress = "fe80::1";

            IPv6Address address = IPv6Address.FromString(ipAddress);

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            Mock<IDeviceService> deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(x => x.GetIPv6LinkLocalAddressFromDevice(deviceId)).Returns(address).Verifiable();

            var resolver = new DeviceBasedDHCPv6PeerAddressResolver(deviceServiceMock.Object);

            resolver.ApplyValues(new Dictionary<String, String> {
               { "DeviceId", deviceId.ToString() },
            }, serializerMock.Object);

            var packet = DHCPv6RelayPacket.AsOuterRelay(new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2")),
                true,1,random.GetIPv6Address(), random.GetIPv6Address(), Array.Empty<DHCPv6PacketOption>(), DHCPv6RelayPacket.AsInnerRelay(
             true, 0, IPv6Address.FromString("fe80::1"), shouldMeetCondition == true ? IPv6Address.FromString(ipAddress) : IPv6Address.FromString("2004::1"), new DHCPv6PacketOption[]
            {
            }, DHCPv6Packet.AsInner(random.NextUInt16(), DHCPv6PacketTypes.Solicit, Array.Empty<DHCPv6PacketOption>())));

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DeviceBasedDHCPv6PeerAddressResolver", new[] {
                new ScopeResolverPropertyDescription("DeviceId",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Device),
            });

            var resolver = new DeviceBasedDHCPv6PeerAddressResolver(Mock.Of<IDeviceService>());
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }

    }
}
