using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes;
using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;
using static Beer.DaAPI.Shared.Responses.CommenResponses.V1;

namespace Beer.DaAPI.Shared.Responses
{
    public static class DHCPv4LeasesResponses
    {
        public static class V1
        {
            public class DHCPv4ScopeOverview
            {
                public Guid Id { get; set; }
                public String Name { get; set; }
            }

            public class DHCPv4LeaseOverview : ILeaseOverview
            {
                public Guid Id { get; set; }
                public DateTime Started { get; set; }
                public DateTime ExpectedEnd { get; set; }
                public DateTime RenewTime { get; set; }
                public DateTime ReboundTime { get; set; }
                public Byte[] UniqueIdentifier { get; set; }
                public Byte[] MacAddress { get; set; }
                public String Address { get; set; }
                public ScopeOverview Scope { get; set; }
                public LeaseStates State { get; set; }
            }
        }
    }
}
