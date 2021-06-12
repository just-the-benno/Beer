using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using Xunit;
using Xunit.Sdk;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;

namespace Beer.DaAPI.UnitTests.Core.Packets.DHCPv6
{
    public class DHCPv4PacketTester
    {

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        [Fact]
        public void Blub()
        {
            String input = "0101060115430d42062c0000000000000000000000000000c2229c0202a0574ff12f00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000638253633501013d070102a0574ff12f390205003204c23763440c13524f555445524c414e434f4d313739335641573c0e4c414e434f4d20313739335641573707017903060f2bd4520d0103312f320206082697683201ff0000000000000000";
            Byte[] content = StringToByteArray(input);

            DHCPv4Packet packet = DHCPv4Packet.FromByteArray(content, new IPv4HeaderInformation(IPv4Address.FromString("192.168.10.0"), IPv4Address.FromString("192.168.10.10")));
            Assert.True(packet.IsValid);
        }

    }
}
