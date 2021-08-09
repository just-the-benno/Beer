using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Shared.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;

namespace Beer.DaAPI.Shared.Responses
{
    public static class TracingResponses
    {
        public static class V1
        {
            public class TracingStreamOverview
            {
                public Guid Id { get; set; }
                public DateTime Timestamp { get; set; }
                public Int32 ModuleIdentifier { get; set; }
                public Int32 ProcedureIdentifier { get; set; }
                public IDictionary<String, String> FirstEntryData { get; set; }
                public Int32 RecordAmount { get; set; }
                public Boolean IsInProgress { get; set; }
            }

            public class TracingStreamRecord
            {
                public String Identifier { get; set; }

                public DateTime Timestamp { get; set; }
                public Guid? EntityId { get; set; }

                public Dictionary<String, String> AddtionalData { get; set; }
            }
        }
    }
}
