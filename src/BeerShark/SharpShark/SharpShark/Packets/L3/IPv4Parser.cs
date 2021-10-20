using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class IPv4PacketParser : IPacketParser
    {
        public bool CanParse(PacketStackInfo previousStackElement, Memory<byte> content) =>
            (previousStackElement is EthernetFrame frame && frame.Type == (UInt16)EthnernetPayloadTypes.IPv4)

            ;

        public PacketParserOutput ParsePacket(PacketStackInfo previousStackElement, Memory<byte> content)
        {
            var span = content.Slice(0, 20).Span;

            Byte headerLength = span[0];
            headerLength &= 0b_0000_1111;
            headerLength = (Byte)(headerLength * 32 / 8);
            if (headerLength != 20)
            {
                span = content.Slice(0, headerLength).Span;
            }

            Byte tos = span[1];
            UInt16 packetLength = ByteHelper.ConvertToUInt16(span, 2);
            UInt16 identification = ByteHelper.ConvertToUInt16(span, 4);

            Byte flags = span[6];
            Boolean dontFragement = (flags & 0b_0100_0000) == 0b_0100_0000;
            Boolean moreFragement = (flags & 0b_0010_0000) == 0b_0010_0000;

            UInt16 fragmentOffset = ByteHelper.ConvertToUInt16(span, 6);
            fragmentOffset &= 0b_0001_1111_1111_1111;

            Byte ttl = span[8];
            Byte protocol = span[9];

            UInt16 checksum = ByteHelper.ConvertToUInt16(span, 10);

            Byte[] sourceAddress = span.Slice(12, 4).ToArray();
            Byte[] destinationAddress = span.Slice(16, 4).ToArray();

            Int32 diff = content.Length - packetLength;

            if (diff > 0)
            {
                //padded packet detected
                return new PacketParserOutput(-diff, Array.Empty<PacketStackInfo>(), false);
            }

            var packet = new IPv4Packet(headerLength, tos, packetLength, identification,
                dontFragement, moreFragement, fragmentOffset, ttl, protocol, checksum,
                new IPv4Address(sourceAddress), new IPv4Address(destinationAddress), content, previousStackElement.Packet);

            return new PacketParserOutput(headerLength, new[] { packet }, false);
        }
    }
}
