using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public interface IPacketStackBuilder
    {
        void AddParser(IPacketParser parser);
        void AddDataLinkParser(IDatalinkPacketParser parser);

        IReadOnlyList<PacketStackInfo> BuildPacketStack(Packet packet);

    }
}
