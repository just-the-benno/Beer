using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    internal class FragmentationBasedEthernetControlWordPacket : EthernetControlWordPacket
    {
        public FragmentationBasedEthernetControlWordPacket(Byte flags, Byte fragmentBits, Byte length, UInt16 sequenceNumber, Memory<Byte> content, Packet packet) : base(sequenceNumber, content, packet)
        {
            Flags = flags;
            FragmentBits = fragmentBits;
            Length = length;
        }

        public Byte Flags { get; init; }
        public Byte FragmentBits { get; init; }
        public Byte Length { get; init; }
    }
}
