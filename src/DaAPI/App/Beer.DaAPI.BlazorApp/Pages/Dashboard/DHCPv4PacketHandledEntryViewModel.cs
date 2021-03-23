using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.Dashboard
{
    public class DHCPv4PacketHandledEntryViewModel : DHCPv4PacketHandledEntry
    {
        public DHCPv4ScopeItem Scope { get; set; }
    }
}
