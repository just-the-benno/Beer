using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.Dashboard
{
    public class DHCPv6PacketHandledEntryViewModel : DHCPv6PacketHandledEntry
    {
        public DHCPv6ScopeItem Scope { get; set; }
    }
}
