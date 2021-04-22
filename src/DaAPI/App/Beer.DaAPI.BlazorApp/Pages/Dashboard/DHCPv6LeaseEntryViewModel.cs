using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;


namespace Beer.DaAPI.BlazorApp.Pages.Dashboard
{
    public interface IViewModelLeaseEntry : ILeaseEntry
    {
        DHCPLeaseStates State
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                if (now > ExpectedRebindingAt)
                {
                    return DHCPLeaseStates.Rebinding;
                }
                else if (now > ExpectedRenewalAt)
                {
                    return DHCPLeaseStates.Renewing;
                }
                else
                {
                    return DHCPLeaseStates.Active;
                }
            }
        }
    }

    public class DHCPv6LeaseEntryViewModel : DHCPv6LeaseEntry, IViewModelLeaseEntry
    {
        public DHCPv6ScopeItem Scope { get; set; }

        public String GetAsSeachString() => $"{Scope.Name} {Address} {Prefix}/{PrefixLength}";
    }
}
