using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class MPLSPacket : PacketStackInfo
    {
        public UInt32 Label { get; init; }
        public Byte TTL { get; init; }
        public Boolean IsBottomOfStack { get; init; }
        public Byte ExperimentalBits { get; init; }

        public MPLSPacket(UInt32 label, Byte ttl, Boolean isBottomOfStack, byte experimentalBits, Memory<byte> content, Packet packet) : base(content, packet)
        {
            Label = label;
            TTL = ttl;
            IsBottomOfStack = isBottomOfStack;
            ExperimentalBits = experimentalBits;
        }
    }
}
