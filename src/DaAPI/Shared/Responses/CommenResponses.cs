using Beer.DaAPI.Core.Scopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Shared.Responses
{
    public static class CommenResponses
    {
        public static class V1
        {
            public interface ILeaseOverview
            {
                public Guid Id { get; set; }
                public DateTime Started { get; set; }
                public DateTime ExpectedEnd { get; set; }
                public DateTime RenewTime { get; set; }
                public DateTime ReboundTime { get; set; }
                public Byte[] UniqueIdentifier { get; set; }
                public String Address { get; set; }
                public ScopeOverview Scope { get; set; }
                public LeaseStates State { get; set; }
            }

            public class ScopeOverview
            {
                public Guid Id { get; set; }
                public String Name { get; set; }
            }

            public class LeaseEventOverview
            {
                public ScopeOverview Scope { get; set; }
                public Guid PacketHandledId { get; set; }
                public DateTime Timestamp { get; set; }
                public String Address { get; set; }
                public String EventName { get; set; }
                public String EventType { get; set; }
                public String EventData { get; set; }
                public Boolean HasResponsePacket { get; set; }
            }

        }
    }
}
