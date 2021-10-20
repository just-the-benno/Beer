using SharpShark.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Analyzer
{
    public class IPV4AddressContainsAnalyzer : IPacketAnalyzer<IEnumerable<IPv4Address>>
    {
        public IEnumerable<IPv4Address> Process(IEnumerable<Packet> input)
        {
            HashSet<IPv4Address> addresses = new();
            foreach (Packet packet in input)
            {
                var packets = packet.Stack.GetPacketsOfType<IPv4Packet>();
                foreach (var item in packets)
                {
                    addresses.Add(item.Source);
                    addresses.Add(item.Destionation);
                }
            }

            return addresses;
        }
    }
}
