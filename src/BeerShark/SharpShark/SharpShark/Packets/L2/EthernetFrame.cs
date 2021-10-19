using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class EthernetFrame : PacketStackInfo
    {
        public Byte[] SourceAddress { get; init; } 
        public Byte[] DestionationAddress { get; init; } 
        public UInt16 Type { get; init; }

        public EthernetFrame(Byte[] destionationAddress, Byte[] sourceAddress, UInt16 type,  Packet packet, Memory<byte> data) : base(data, packet)
        {
            SourceAddress = sourceAddress;
            DestionationAddress = destionationAddress;
            Type = type;
        }

    }
}
