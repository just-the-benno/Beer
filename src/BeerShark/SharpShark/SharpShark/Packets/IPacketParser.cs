using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public record PacketParserOutput(Int32 HeaderLength, IEnumerable<PacketStackInfo> StackInfos, Boolean IsLast);

    public interface IPacketParser
    {
        PacketParserOutput ParsePacket(PacketStackInfo previousStackElement, Memory<Byte> content);
        Boolean CanParse(PacketStackInfo previousStackElement, Memory<Byte> content);
    }

    public interface IDatalinkPacketParser
    {
        Boolean CanParse(UInt32 datalinkIdentifier);
        PacketParserOutput ParsePacket(Packet packet, Memory<Byte> content);

    }
}
