using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Shared.Commands
{
    public static class PacketMonitorRequest
    {
        public static class V1
        {
            public interface IPacketFilter<T> where T : struct
            {
                Int32 Amount { get; }
                Int32 Start { get; }

                DateTime? From { get; }
                DateTime? To { get; }
                String MacAddress { get; }
                String RequestedIp { get; }
                String LeasedIp { get; }
                Boolean? Filtered { get; }
                Boolean? Invalid { get; }

                String SourceAddress { get; }
                String DestinationAddress { get; }
                Guid? ScopeId { get; }
                Boolean? IncludeScopeChildren { get; }
                T? RequestMessageType { get; }
                T? ResponseMessageType { get; }
                Boolean? HasAnswer { get; }
                Int32? ResultCode { get; }
            }

            public record DHCPv4PacketFilter : IPacketFilter<DHCPv4MessagesTypes>
            {
                public Int32 Amount { get; init; }
                public Int32 Start { get; init; }

                public DateTime? From { get; init; }
                public DateTime? To { get; init; }
                public String MacAddress { get; init; }
                public String RequestedIp { get; init; }
                public String LeasedIp { get; init; }
                public Boolean? Filtered { get; init; }
                public Boolean? Invalid { get; init; }

                public String SourceAddress { get; set; }
                public String DestinationAddress { get; set; }
                public Guid? ScopeId { get; init; }
                public Boolean? IncludeScopeChildren { get; init; }
                public DHCPv4MessagesTypes? RequestMessageType { get; init; }
                public DHCPv4MessagesTypes? ResponseMessageType { get; init; }
                public Boolean? HasAnswer { get; init; }
                public Int32? ResultCode { get; set; }
            }

            public record DHCPv6PacketFilter : IPacketFilter<DHCPv6PacketTypes>
            {
                public Int32 Amount { get; init; }
                public Int32 Start { get; init; }

                public DateTime? From { get; init; }
                public DateTime? To { get; init; }
                public String MacAddress { get; init; }
                public String RequestedIp { get; init; }
                public String RequestedPrefix { get; set; }
                public String LeasedIp { get; init; }
                public String LeasedPrefix { get; set; }
                public Boolean? Filtered { get; init; }
                public Boolean? Invalid { get; init; }

                public String SourceAddress { get; set; }
                public String DestinationAddress { get; set; }
                public Guid? ScopeId { get; init; }
                public Boolean? IncludeScopeChildren { get; init; }
                public DHCPv6PacketTypes? RequestMessageType { get; init; }
                public DHCPv6PacketTypes? ResponseMessageType { get; init; }
                public Boolean? HasAnswer { get; init; }
                public Int32? ResultCode { get; set; }
            }
        }
    }
}
