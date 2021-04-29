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
    public class DeviceBasedDeviceBasedDHCPv6SimpleZyxelIESResolverTester
    {
        [Fact]
        public void HasUniqueIdentifier()
        {
            var resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            Assert.True(resolver.HasUniqueIdentifier);
        }

        private Byte[] GetExpectedByteSequence(Int32 slotId, Int32 portId) => (new ASCIIEncoding()).GetBytes($"{slotId}/{portId}");

        private DHCPv6Packet GetPacket(Random random, Byte[] remoteId, Int32 slotId, Int32 portId, Int32 enterpriseId = 0, Boolean includeRelevantOptions = true)
        {
            IPv6HeaderInformation headerInformation =
                new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            var packetOptions = new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                };

            DHCPv6Packet innerPacket = DHCPv6Packet.AsInner(random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            DHCPv6Packet innerRelayPacket = DHCPv6RelayPacket.AsInnerRelay(true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[]
                {
                    new DHCPv6PacketIdentifierOption(DHCPv6PacketOptionTypes.ClientIdentifier, new UUIDDUID(random.NextGuid())),
                    includeRelevantOptions == true ? (DHCPv6PacketOption)( new DHCPv6PacketRemoteIdentifierOption((UInt32)enterpriseId,remoteId)) : new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                    includeRelevantOptions == true ? (DHCPv6PacketOption)( new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, GetExpectedByteSequence(slotId,portId))) : new DHCPv6PacketTimeOption(DHCPv6PacketOptionTypes.ElapsedTime,10,DHCPv6PacketTimeOption.DHCPv6PacketTimeOptionUnits.Minutes)

                }, innerPacket);

            DHCPv6Packet outerRelayPacket = DHCPv6RelayPacket.AsOuterRelay(headerInformation, true, 1, random.GetIPv6Address(), random.GetIPv6Address(),
                new DHCPv6PacketOption[]
                {
                                new DHCPv6PacketByteArrayOption(DHCPv6PacketOptionTypes.InterfaceId, random.NextBytes(10)),
                                new DHCPv6PacketRemoteIdentifierOption(9,random.NextBytes(12))
                }, innerRelayPacket);

            return outerRelayPacket;
        }

        [Fact]
        public void GetUniqueIdentifier()
        {
            Random random = new Random();
            var resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            Byte[] macAddress = random.NextBytes(6);
            Int32 slotId = random.NextByte();
            Int32 portId = random.NextByte();

            DHCPv6Packet packet = GetPacket(random, macAddress, slotId, portId);

            Byte[] actual = resolver.GetUniqueIdentifier(packet);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(0, actual[i]);

            }

            for (int i = 0; i < macAddress.Length; i++)
            {
                Assert.Equal(macAddress[i], actual[i + 4]);
            }

            Byte[] expectedInterfaceValue = GetExpectedByteSequence(slotId, portId);
            for (int i = 0; i < expectedInterfaceValue.Length; i++)
            {
                Assert.Equal(expectedInterfaceValue[i], actual[i + macAddress.Length + 4]);
            }
        }

        [Theory]
        [InlineData("2", "1", "1", "c690752b-56a3-45bd-a7d9-bc4a151aef69", true)]
        [InlineData("0", "1", "1", "c690752b-56a3-45bd-a7d9-bc4a151aef69", true)]
        [InlineData("0", "4", "2", "c690752b-56a3-45bd-a7d9-bc4a151aef69", true)]
        [InlineData("-1", "0", "1", "c690752b-56a3-45bd-a7d9-bc4a151aef69", false)]
        [InlineData("2", "0", "1", "c690752b-56a3-45bd-a7d9-bc4a151aef69", false)]
        [InlineData("2", "1", "0", "c690752b-56a3-45bd-a7d9-bc4a151aef69", false)]
        [InlineData("2", "-1", "1", "c690752b-56a3-45bd-a7d9-bc4a151aef69", false)]
        [InlineData("2", "1", "-1", "c690752b-56a3-45bd-a7d9-bc4a151aef69", false)]
        [InlineData("2", "1", "1", "c690752b-56a3-45bd-a7d9-bc4a151aef6", false)]
        [InlineData("2", "1", "1", "", false)]
        public void ArePropertiesAndValuesValid(String index, String slotId, String portId, String deviceId, Boolean shouldBeValid)
        {
            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);

            serializerMock.Setup(x => x.Deserialze<String>(index)).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(slotId)).Returns(slotId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(portId)).Returns(portId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(deviceId)).Returns(deviceId).Verifiable();

            var resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            Boolean actual = resolver.ArePropertiesAndValuesValid(new Dictionary<String, String> {
               { "Index", index },
               { "SlotId", slotId },
               { "PortId", portId },
               { "DeviceId", deviceId },
            }, serializerMock.Object);

            Assert.Equal(shouldBeValid, actual);
        }

        [Fact]
        public void ArePropertiesAndValuesValid_KeyIsMissing()
        {
            var input = new[]{
                new  Dictionary<String,String>{
                    { "Index1", "0" },
                    { "SlotId", "0" },
                    { "PortId", "0" },
                    { "DeviceId", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId1", "0" },
                    { "PortId", "0" },
                    { "DeviceId", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "PortId1", "0" },
                    { "DeviceId", "0" },
                },
               new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "PortId", "0" },
                    { "DeviceId1", "0" },
                },
                new  Dictionary<String,String>{
                    { "SlotId", "0" },
                    { "PortId", "0" },
                    { "DeviceId", "0" },
                },
                 new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "PortId", "0" },
                    { "DeviceId", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "DeviceId", "0" },
                },
                new  Dictionary<String,String>{
                    { "Index", "0" },
                    { "SlotId", "0" },
                    { "PortId", "0" },
                },
                };

            var resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

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

            UInt16 index = (UInt16)random.Next(0, 10);
            UInt16 slotId = (UInt16)random.Next(0, 10);
            UInt16 portId = (UInt16)random.Next(0, 10);

            Guid deviceId = random.NextGuid();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt16>(index.ToString())).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(slotId.ToString())).Returns(slotId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(portId.ToString())).Returns(portId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(deviceId.ToString())).Returns(deviceId).Verifiable();

            var resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            resolver.ApplyValues(new Dictionary<String, String> {
                    { "Index", index.ToString() },
                    { "SlotId", slotId.ToString() },
                    { "PortId", portId.ToString() },
                    { "DeviceId", deviceId.ToString() },
            }, serializerMock.Object);

            Assert.Equal(index, resolver.Index);
            Assert.Equal(slotId, resolver.SlotId);
            Assert.Equal(portId, resolver.PortId);
            Assert.Equal(deviceId, resolver.DeviceId);

            serializerMock.Verify();

            Dictionary<String, String> expectedValues = new Dictionary<string, string>
            {
                { "Index", index.ToString() },
                { "SlotId", slotId.ToString() },
                { "PortId", portId.ToString() },
                { "DeviceId", deviceId.ToString() },
            };

            Assert.Equal(expectedValues.ToArray(), resolver.GetValues().ToArray());
            Assert.Equal(expectedValues, resolver.GetValues(), new NonStrictDictionaryComparer<String, String>());

        }

        private static (UInt16 slotId, UInt16 portId) GetValidResolver(Random random, UInt16 index, out Guid deviceId, out Mock<ISerializer> serializerMock, out Mock<IDeviceService> deviceServiceMock, out DeviceBasedDHCPv6SimpleZyxelIESResolver resolver)
        {
            UInt16 slotId = (UInt16)random.Next(0, 10);
            UInt16 portId = (UInt16)random.Next(0, 10);

            Guid realDeviceId = random.NextGuid();

            deviceId = realDeviceId;

            String value = random.GetAlphanumericString();

            serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<UInt16>(index.ToString())).Returns(index).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(slotId.ToString())).Returns(slotId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<UInt16>(portId.ToString())).Returns(portId).Verifiable();
            serializerMock.Setup(x => x.Deserialze<Guid>(realDeviceId.ToString())).Returns(deviceId).Verifiable();

            deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);

            resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(deviceServiceMock.Object);
            resolver.ApplyValues(new Dictionary<String, String> {
                    { "Index", index.ToString() },
                    { "SlotId", slotId.ToString() },
                    { "PortId", portId.ToString() },
                    { "DeviceId", realDeviceId.ToString() },
            }, serializerMock.Object);

            serializerMock.Verify();

            return (slotId, portId);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PacketMeetsCondition(Boolean shouldMeetCondition)
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 0, out Guid deviceId, out Mock<ISerializer> serializerMock, out Mock<IDeviceService> deviceServiceMock, out DeviceBasedDHCPv6SimpleZyxelIESResolver resolver);

            Byte[] remoteIdentifierAsBytes = random.NextBytes(6);

            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns( shouldMeetCondition == true ? remoteIdentifierAsBytes : random.NextBytes(6)).Verifiable();
            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes, slotId, portId);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldMeetCondition, result);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_DifferentEnterpriseNumber()
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 0, out Guid deviceId, out Mock<ISerializer> serializerMock, out Mock<IDeviceService> deviceServiceMock, out DeviceBasedDHCPv6SimpleZyxelIESResolver resolver);

            Byte[] remoteIdentifierAsBytes = random.NextBytes(6);

            deviceServiceMock.Setup(x => x.GetMacAddressFromDevice(deviceId)).Returns(remoteIdentifierAsBytes).Verifiable();

            DHCPv6Packet packet = GetPacket(random, remoteIdentifierAsBytes, slotId, portId,random.Next());

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.True(result);

            serializerMock.Verify();
            deviceServiceMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_NotRelay()
        {
            Random random = new Random();

            Mock<ISerializer> serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            DeviceBasedDHCPv6SimpleZyxelIESResolver resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));

            IPv6HeaderInformation headerInformation =
                new IPv6HeaderInformation(IPv6Address.FromString("fe80::1"), IPv6Address.FromString("fe80::2"));

            var packetOptions = new List<DHCPv6PacketOption>
                {
                    new DHCPv6PacketTrueOption(DHCPv6PacketOptionTypes.RapitCommit),
                };

            DHCPv6Packet packet = DHCPv6Packet.AsOuter(headerInformation, random.NextUInt16(),
                DHCPv6PacketTypes.Solicit, packetOptions);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_IndexNotPresented()
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 2, out Guid _, out Mock<ISerializer> serializerMock, out Mock<IDeviceService> _, out DeviceBasedDHCPv6SimpleZyxelIESResolver resolver);

            DHCPv6Packet packet = GetPacket(random, random.NextBytes(6), slotId, portId);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);

            serializerMock.Verify();
        }

        [Fact]
        public void PacketMeetsCondition_False_OptionNotPresented()
        {
            Random random = new Random();

            var (slotId, portId) = GetValidResolver(random, 0, out Guid _, out Mock<ISerializer> serializerMock, out Mock<IDeviceService> _, out DeviceBasedDHCPv6SimpleZyxelIESResolver resolver);

            DHCPv6Packet packet = GetPacket(random, random.NextBytes(6), slotId, portId,0, false);

            Boolean result = resolver.PacketMeetsCondition(packet);
            Assert.False(result);
        }

        [Fact]
        public void GetDescription()
        {
            var expected = new ScopeResolverDescription("DeviceBasedDHCPv6SimpleZyxelIESResolver", new[] {
            new ScopeResolverPropertyDescription("Index",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32),
            new ScopeResolverPropertyDescription("SlotId",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32),
            new ScopeResolverPropertyDescription("PortId",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.UInt32),
            new ScopeResolverPropertyDescription("DeviceId",ScopeResolverPropertyDescription.ScopeResolverPropertyValueTypes.Device),
            });

            var resolver = new DeviceBasedDHCPv6SimpleZyxelIESResolver(Mock.Of<IDeviceService>(MockBehavior.Strict));
            var actual = resolver.GetDescription();

            Assert.Equal(expected, actual);
        }

    }
}
