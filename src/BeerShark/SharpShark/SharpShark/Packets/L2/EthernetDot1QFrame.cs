using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class EthernetDot1QFrame : EthernetFrame
    {
        public EthernetDot1QFrame(byte[] destionationAddress, byte[] sourceAddress, UInt16 vlanId, Byte cos, UInt16 type, Memory<Byte> content, Packet packet) : base(destionationAddress, sourceAddress, type, packet, content)
        {
            VlanId = vlanId;
            Cos = cos;
            Type = type;
        }

        public UInt16 VlanId { get; init; }
        public Byte Cos { get; init; }
    }
}
