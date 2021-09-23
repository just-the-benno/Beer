using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Shared.Responses
{
    public static class PacketMonitorResponses
    {
        public static class V1
        {
            public class ScopeOverview
            {
                public String Name { get; set; }
                public Guid Id { get; set; }
            }

            public interface IPacketOverview
            {
                Guid Id { get; set; }
                String MacAddress { get; set; }
                DateTime Timestamp { get; set; }
                String SourceAddress { get; set; }
                String DestinationAddress { get; set; }
                Int32 ResultCode { get; set; }
                String RequestedIp { get; set; }
                String LeasedIp { get; set; }
                Boolean Filtered { get; set; }
                Boolean Invalid { get; set; }

                ScopeOverview Scope { get; set; }

                Int32 RequestSize { get; set; }
                Int32 ResponseSize { get; set; }
            }

            public class DHCPv4PacketOverview : IPacketOverview
            {
                public Guid Id { get; set; }
                public String MacAddress { get; set; }
                public DateTime Timestamp { get; set; }
                public String SourceAddress { get; set; }
                public String DestinationAddress { get; set; }
                public DHCPv4MessagesTypes RequestMessageType { get; set; }
                public DHCPv4MessagesTypes? ResponseMessageType { get; set; }
                public Int32 ResultCode { get; set; }
                public String RequestedIp { get; set; }
                public String LeasedIp { get; set; }
                public Boolean Filtered { get; set; }
                public Boolean Invalid { get; set; }

                public ScopeOverview Scope { get; set; }

                public Int32 RequestSize { get; set; }
                public Int32 ResponseSize { get; set; }

            }

            public class DHCPv6PrefixModel
            {
                public String Network { get; set; }
                public Int32 Length { get; set;  }
            }

            public class DHCPv6PacketOverview : IPacketOverview
            {
                public Guid Id { get; set; }
                public String MacAddress { get; set; }
                public DateTime Timestamp { get; set; }
                public String SourceAddress { get; set; }
                public String DestinationAddress { get; set; }
                public DHCPv6PacketTypes RequestMessageType { get; set; }
                public DHCPv6PacketTypes? ResponseMessageType { get; set; }
                public Int32 ResultCode { get; set; }
                public String RequestedIp { get; set; }
                public DHCPv6PrefixModel RequestedPrefix { get; set; }
                public String LeasedIp { get; set; }
                public DHCPv6PrefixModel LeasedPrefix { get; set; }
                public Boolean Filtered { get; set; }
                public Boolean Invalid { get; set; }

                public ScopeOverview Scope { get; set; }

                public Int32 RequestSize { get; set; }
                public Int32 ResponseSize { get; set; }
            }

            public class PacketInfo
            {
                public Byte[] Content { get; set; }
                public String Source { get; set; }
                public String Destination { get; set; }
            }

            public enum PacketStatisticTimePeriod
            {
                LastHour = 0,
                LastDay = 1,
                LastWeek = 2,
            }

            public class IncomingAndOutgoingPacketStatisticItem
            {
                public Int32 OutgoingPacketAmount { get; set; }
                public Int32 OutgoingPacketTotalSize { get; set; }
                public Int32 IncomingPacketAmount { get; set; }
                public Int32 IncomingPacketTotalSize { get; set; }
            }

        }
    }
}
