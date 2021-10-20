using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class UnknownPacketStackInfo : PacketStackInfo
    {
        public UnknownPacketStackInfo(Memory<Byte> data, Packet packet) : base(data, packet)
        {

        }

    }
}
