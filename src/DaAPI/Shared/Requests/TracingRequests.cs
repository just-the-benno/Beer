using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Beer.DaAPI.Shared.Requests
{
    public static class TracingRequests
    {
        public static class V1
        {
            public class FilterTracingRequest
            {
                public Int32? ModuleIdentifier { get; set; }
                public Int32? ProcedureIdentifier { get; set; }
                public DateTime? StartedBefore { get; set; }
                public Int32 Amount { get; set; } = 30;
                public Int32 Start { get; set; } = 0;
                public Guid? EntitiyId { get; set; }   
            }
        }
    }
}
