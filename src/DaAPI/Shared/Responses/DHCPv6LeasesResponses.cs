using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1.DHCPv6ScopeAddressPropertyReqest;
using static Beer.DaAPI.Shared.Responses.CommenResponses.V1;

namespace Beer.DaAPI.Shared.Responses
{
    public static class DHCPv6LeasesResponses
    {
        public static class V1
        {
            public class PrefixOverview
            {
                public String Address { get; set; }
                public Byte Mask { get; set; }
            }

            public class DHCPv6LeaseOverview : ILeaseOverview
            {
                public Guid Id { get; set; }
                public DateTime Started { get; set; }
                public DateTime ExpectedEnd { get; set; }
                public Byte[] UniqueIdentifier { get; set; }
                public Byte[] ClientIdentifier { get; set; }
                public String Address { get; set; }
                public PrefixOverview Prefix { get; set; }
                public ScopeOverview Scope { get; set; }
                public LeaseStates State { get; set; }
                public DateTime ReboundTime { get; set; }
                public DateTime RenewTime { get; set; }
            }

        }
    }
}
