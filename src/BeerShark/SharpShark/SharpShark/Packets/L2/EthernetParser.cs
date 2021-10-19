using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public enum EthernetTypes : ushort
    {
        Dot1q = 0x8100,
        MPLS = 0x8847,
        IPv4 = 0x0800,
        IPv6 = 0x0400,
    }

    public class EthernetParser : IDatalinkPacketParser
    {
        public Boolean CanParse(uint datalinkIdentifier) => LinkLayerHeaderTypes.Ethernet == (LinkLayerHeaderTypes)datalinkIdentifier;

        public PacketParserOutput ParsePacket(Packet packet, Memory<byte> content)
        {
            var span = content.Span;

            UInt16 type = ByteHelper.ConvertToUInt16(span, 6 + 6);


            EthernetFrame frame = new (span.Slice(0, 6).ToArray(), span.Slice(6, 6).ToArray(), type, packet, content);

            if ((EthernetTypes)type != EthernetTypes.Dot1q)
            {
                return new PacketParserOutput(6 + 6 + 2, new[] { frame }, false);
            }

            Byte cos = span[6 + 6 + 2];
            cos &= 0b11_10_00_00;
            cos >>= 5;

            UInt16 vlan = ByteHelper.ConvertToUInt16(span, 12 + 2);
            vlan &= 0b00001111_11111111;

            UInt16 innerType = ByteHelper.ConvertToUInt16(span, 6 + 6 + 2 + 2);

            EthernetDot1QFrame dot1qFrame = new(span.Slice(0, 6).ToArray(), span.Slice(6, 6).ToArray(), vlan, cos, innerType, content[(6 + 6 + 2)..], packet);
            return new PacketParserOutput(14 + 2 + 2, new PacketStackInfo[] { frame, dot1qFrame }, false);
        }
    }
}
