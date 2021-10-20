using SharpShark.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Analyzer
{
    public interface IPacketAnalyzer<TOutput>
    {
        TOutput Process(IEnumerable<Packet> input);

    }
}
