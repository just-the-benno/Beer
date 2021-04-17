using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static Beer.DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;

namespace Beer.DaAPI.UnitTests.Core.Common.DHCPv4
{
    public class DHCPv4ClientIdentifierTester
    {
        [Fact]
        public void FromOptionData_HWAddress()
        {
            Random random = new Random();
            Byte[] identifierValue = new Byte[7];
            random.NextBytes(identifierValue);
            identifierValue[0] =
                (Byte)DHCPv4PacketHardwareAddressTypes.Ethernet;

            DHCPv4ClientIdentifier identifier = DHCPv4ClientIdentifier.FromOptionData(identifierValue);

            Assert.Equal(
                identifierValue.Skip(1).ToArray(),
                identifier.HwAddress);

            Assert.Equal(Beer.DaAPI.Core.Common.DUID.Empty, identifier.DUID);
            Assert.True(String.IsNullOrEmpty(identifier.IdentifierValue));
            Assert.Equal((Byte)DHCPv4PacketHardwareAddressTypes.Ethernet, identifier.HardwareAddressType);
        }

        [Fact]
        public void FromOptionData_FromDUID()
        {
            Random random = new Random();
            Guid duidValue = random.NextGuid();

            UUIDDUID duid = new UUIDDUID(duidValue);

            UInt32 iaid = random.NextUInt32();
            Byte[] identifierValue = duid.GetAsByteStream();

            Byte[] input = ByteHelper.ConcatBytes(new Byte[] { 0xff }, ByteHelper.GetBytes(iaid), identifierValue);

            DHCPv4ClientIdentifier identifier = DHCPv4ClientIdentifier.FromOptionData(input);

            Assert.Empty(identifier.HwAddress);
            Assert.Equal(duid, identifier.DUID);
            Assert.Equal(iaid, identifier.IaId);

            Assert.True(String.IsNullOrEmpty(identifier.IdentifierValue));
        }

        [Fact]
        public void Equals_WithoutDuid()
        {
            Random random = new Random();
            Byte[] firstHWAddress = random.NextBytes(12);
            Byte[] secondHWAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromHwAddress((Byte)DHCPv4PacketHardwareAddressTypes.Ethernet, firstHWAddress);
            DHCPv4ClientIdentifier secondIdentifier = DHCPv4ClientIdentifier.FromHwAddress((Byte)DHCPv4PacketHardwareAddressTypes.Ethernet, firstHWAddress);
            DHCPv4ClientIdentifier thirdIdentifier = DHCPv4ClientIdentifier.FromHwAddress((Byte)DHCPv4PacketHardwareAddressTypes.Ethernet, secondHWAddress);

            Assert.Equal(firstIdentifier, secondIdentifier);
            Assert.NotEqual(firstIdentifier, thirdIdentifier);
        }

        [Fact]
        public void Equals_WithIdentifier()
        {
            Random random = new Random();

            Byte[] hwAddress = random.NextBytes(12);

            String firstIdentifierValue = random.GetAlphanumericString();
            String secondIdentifierValue = random.GetAlphanumericString();

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromIdentifierValue(firstIdentifierValue);
            DHCPv4ClientIdentifier secondIdentifier = DHCPv4ClientIdentifier.FromIdentifierValue(firstIdentifierValue);
            DHCPv4ClientIdentifier thirdIdentifier = DHCPv4ClientIdentifier.FromIdentifierValue(secondIdentifierValue);

            firstIdentifier = firstIdentifier.AddHardwareAddress(hwAddress);
            secondIdentifier = secondIdentifier.AddHardwareAddress(hwAddress);
            thirdIdentifier = thirdIdentifier.AddHardwareAddress(hwAddress);

            Assert.Equal(firstIdentifier, secondIdentifier);
            Assert.NotEqual(firstIdentifier, thirdIdentifier);
        }

        [Fact]
        public void Equals_BasedOnDuid()
        {
            Guid firstGuid = Guid.NewGuid();
            Guid secondGuid = Guid.NewGuid();

            UInt32 iaid = 252132525;

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, new UUIDDUID(firstGuid));
            DHCPv4ClientIdentifier secondIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, new UUIDDUID(firstGuid));
            DHCPv4ClientIdentifier thirdIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, new UUIDDUID(secondGuid));

            Assert.Equal(firstIdentifier, secondIdentifier);
            Assert.NotEqual(firstIdentifier, thirdIdentifier);
        }

        [Fact]
        public void Equals_BasedOnDuid_ButWithDifferentHWAddresses()
        {
            Guid firstGuid = Guid.NewGuid();
            Guid secondGuid = Guid.NewGuid();

            UInt32 iaid = 252132525;

            Random random = new Random();
            Byte[] firstHWAddress = random.NextBytes(12);
            Byte[] secondHWAddress = random.NextBytes(12);
            Byte[] thirdHWAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, new UUIDDUID(firstGuid), firstHWAddress);
            DHCPv4ClientIdentifier secondIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, new UUIDDUID(firstGuid), secondHWAddress);
            DHCPv4ClientIdentifier thirdIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, new UUIDDUID(secondGuid), thirdHWAddress);

            Assert.Equal(firstIdentifier, secondIdentifier);
            Assert.NotEqual(firstIdentifier, thirdIdentifier);
        }

        [Fact]
        public void AddHWAdress_FromDuid()
        {
            UUIDDUID duid = new UUIDDUID(Guid.NewGuid());

            Random random = new Random();
            Byte[] hwAddress = random.NextBytes(12);

            UInt32 iaid = 252132525;

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, duid);
            DHCPv4ClientIdentifier secondIdentifier = firstIdentifier.AddHardwareAddress(hwAddress);

            Assert.Equal(hwAddress, secondIdentifier.HwAddress);
            Assert.Equal(duid, secondIdentifier.DUID);
            Assert.Equal(iaid, secondIdentifier.IaId);
            Assert.Equal((Byte)DHCPv4PacketHardwareAddressTypes.Ethernet, secondIdentifier.HardwareAddressType);
            Assert.True(String.IsNullOrEmpty(secondIdentifier.IdentifierValue));
        }

        [Fact]
        public void AddHWAdress_FromIdentifierValue()
        {
            Random random = new Random();
            String value = random.GetAlphanumericString();

            Byte[] hwAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromIdentifierValue(value);
            DHCPv4ClientIdentifier secondIdentifier = firstIdentifier.AddHardwareAddress(hwAddress);

            Assert.Equal(hwAddress, secondIdentifier.HwAddress);
            Assert.Equal(DaAPI.Core.Common.DUID.Empty, secondIdentifier.DUID);
            Assert.Equal((UInt32)0, secondIdentifier.IaId);
            Assert.Equal((Byte)DHCPv4PacketHardwareAddressTypes.Ethernet, secondIdentifier.HardwareAddressType);
            Assert.Equal(value, secondIdentifier.IdentifierValue);
        }

        [Fact]
        public void AddHWAdress_HwAlreadySet()
        {
            UUIDDUID duid = new UUIDDUID(Guid.NewGuid());

            UInt32 iaid = 252132525;

            Random random = new Random();
            Byte[] hwAddress = random.NextBytes(12);
            Byte[] packetHwAddress = random.NextBytes(12);

            DHCPv4ClientIdentifier firstIdentifier = DHCPv4ClientIdentifier.FromDuid(iaid, duid, hwAddress);
            DHCPv4ClientIdentifier secondIdentifier = firstIdentifier.AddHardwareAddress(packetHwAddress);

            Assert.NotEqual(packetHwAddress, secondIdentifier.HwAddress);
            Assert.Equal(hwAddress, secondIdentifier.HwAddress);
            Assert.Equal(duid, secondIdentifier.DUID);
            Assert.Equal(iaid, secondIdentifier.IaId);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasHardwareAddress(Boolean isSet)
        {
            UUIDDUID duid = new UUIDDUID(Guid.NewGuid());

            Random random = new Random();
            Byte[] hwAddress = random.NextBytes(12);

            UInt32 iaid = 252132525;

            DHCPv4ClientIdentifier identifier;
            if (isSet == true)
            {
                identifier = DHCPv4ClientIdentifier.FromDuid(iaid, duid, hwAddress);
            }
            else
            {
                identifier = DHCPv4ClientIdentifier.FromDuid(iaid, duid);
            }

            Boolean actual = identifier.HasHardwareAddress();
            Assert.Equal(isSet, actual);
        }
    }
}
