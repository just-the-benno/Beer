using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class EthernetControlWordPacket : PacketStackInfo
    {
        public UInt16 SequenceNumber { get; init; }

        public EthernetControlWordPacket(UInt16 sequenceNumber, Memory<byte> content, Packet packet) : base(content, packet)
        {
            SequenceNumber = sequenceNumber;
        }
    }
}
