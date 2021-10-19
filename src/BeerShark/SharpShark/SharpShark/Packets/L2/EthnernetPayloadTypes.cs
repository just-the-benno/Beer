using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public enum EthnernetPayloadTypes : UInt16
    {
        Dot1q = 0x8100,
        MPLS = 0x8847,
        IPv4 = 0x0800,
    }
}
