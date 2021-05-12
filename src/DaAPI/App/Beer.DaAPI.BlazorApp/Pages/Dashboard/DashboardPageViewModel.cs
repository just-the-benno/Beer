using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.Dashboard
{
    public class DashboardViewModelResponse
    {
        public DHCPOverview<DHCPv6LeaseEntryViewModel, DHCPv6PacketHandledEntryViewModel> DHCPv6 { get; set; }
        public DHCPOverview<DHCPv4LeaseEntryViewModel, DHCPv4PacketHandledEntryViewModel> DHCPv4 { get; set; }
        public Int32 AmountOfPipelines { get; set; }
    }

    public enum TypeFilter
    {
        IPv4,
        IPv6,
        Both,
    }

    public enum DHCPLeaseStates
    {
        Pending,
        Active,
        Renewing,
        Rebinding,
        InActive
    }

    public class DashboardPageViewModel
    {
        public DashboardViewModelResponse Response { get; set; }

        public TypeFilter LeaseTypeFilter { get; private set; } = TypeFilter.Both;
        public TypeFilter PacketTypeFilter { get; private set; } = TypeFilter.Both;

        internal void SetLeaseFilter(TypeFilter filter) => LeaseTypeFilter = filter;
        internal void SePacketFilter(TypeFilter filter) => PacketTypeFilter = filter;

        public IDictionary<Guid, DHCPv6ScopeItem> DHCPv6Scopes { get; set; }
        public IDictionary<Guid, DHCPv4ScopeItem> DHCPv4Scopes { get; set; }

        private static DHCPv6ScopeItem GetDefaultDHCPv6Scope() => GetDefaultScopeDHCPv6(Guid.Empty);
        private static DHCPv6ScopeItem GetDefaultScopeDHCPv6(Guid id) => new() { Id = id, Name = String.Empty };

        private static DHCPv4ScopeItem GetDefaultDHCPv4Scope() => GetDefaultScopeDHCPv4(Guid.Empty);
        private static DHCPv4ScopeItem GetDefaultScopeDHCPv4(Guid id) => new() { Id = id, Name = String.Empty };

        public IEnumerable<IPacketEntry> GetPackets(Int32 amount = 30)
        {
            IEnumerable<IPacketEntry> entries = null;
            if (PacketTypeFilter == TypeFilter.Both)
            {
                entries = Response.DHCPv6.Packets.Cast<IPacketEntry>().Union(Response.DHCPv4.Packets);
            }
            else if (PacketTypeFilter == TypeFilter.IPv4)
            {
                entries = Response.DHCPv4.Packets;
            }
            else
            {
                entries = Response.DHCPv6.Packets;
            }

            return entries.OrderByDescending(x => x.Timestamp).Take(amount).ToArray();
        }

        public IEnumerable<IViewModelLeaseEntry> GetLeasesDueToRenew() => Response.DHCPv6.ActiveLeases.Cast<IViewModelLeaseEntry>().Union(Response.DHCPv4.ActiveLeases).Where(x => x.State == DHCPLeaseStates.Rebinding || x.State == DHCPLeaseStates.Renewing).ToArray();

        public IEnumerable<IViewModelLeaseEntry> GetLeases(String searchTerm, Int32 amount = 30)
        {
            var validScopeIds = _scopesToShowForLeases.Where(y => y.Value == true).Select(y => y.Key).ToArray();

            IEnumerable<IViewModelLeaseEntry> entries = null;
            if (String.IsNullOrEmpty(searchTerm) == true)
            {
                if (LeaseTypeFilter == TypeFilter.Both)
                {
                    entries = Response.DHCPv6.ActiveLeases.Cast<IViewModelLeaseEntry>().Union(Response.DHCPv4.ActiveLeases);
                }
                else if (LeaseTypeFilter == TypeFilter.IPv4)
                {
                    entries = Response.DHCPv4.ActiveLeases;
                }
                else
                {
                    entries = Response.DHCPv6.ActiveLeases;
                }
            }
            else
            {
                searchTerm = searchTerm.ToLower().Trim();

                Dictionary<String, IViewModelLeaseEntry> possibleItems = new();
                if (LeaseTypeFilter == TypeFilter.Both || LeaseTypeFilter == TypeFilter.IPv6)
                {
                    foreach (var item in Response.DHCPv6.ActiveLeases)
                    {
                        possibleItems.Add(item.GetAsSeachString().ToLower(), item);
                    }
                }

                if (LeaseTypeFilter == TypeFilter.Both || LeaseTypeFilter == TypeFilter.IPv4)
                {
                    foreach (var item in Response.DHCPv4.ActiveLeases)
                    {
                        possibleItems.Add(item.GetAsSeachString().ToLower(), item);
                    }
                }

                entries = possibleItems.Where(x => x.Key.Contains(searchTerm) == true).Select(x => x.Value);
            }

            return entries.Where(x => validScopeIds.Length == 0 || validScopeIds.Contains(x.ScopeId)).OrderByDescending(x => x.End).Take(amount).ToArray();
        }

        public void SetDHCPv6Scopes(IEnumerable<DHCPv6ScopeItem> scopes)
        {
            DHCPv6Scopes = scopes.ToDictionary(x => x.Id, x => x);

            foreach (var item in Response.DHCPv6.ActiveLeases)
            {
                item.Scope = GetDHCPv6ScopeById(item.ScopeId);
            }

            foreach (var item in Response.DHCPv6.Packets)
            {
                item.Scope = item.ScopeId.HasValue == false ? GetDefaultDHCPv6Scope() : GetDHCPv6ScopeById(item.ScopeId.Value);
            }
        }

        public void SetDHCPv4Scopes(IEnumerable<DHCPv4ScopeItem> scopes)
        {
            DHCPv4Scopes = scopes.ToDictionary(x => x.Id, x => x);

            foreach (var item in Response.DHCPv4.ActiveLeases)
            {
                item.Scope = GetDHCPv4ScopeById(item.ScopeId);
            }

            foreach (var item in Response.DHCPv4.Packets)
            {
                item.Scope = item.ScopeId.HasValue == false ? GetDefaultDHCPv4Scope() : GetDHCPv4ScopeById(item.ScopeId.Value);
            }
        }

        public DHCPv6ScopeItem GetDHCPv6ScopeById(Guid id) => DHCPv6Scopes.ContainsKey(id) == false ? GetDefaultScopeDHCPv6(id) : DHCPv6Scopes[id];
        public DHCPv4ScopeItem GetDHCPv4ScopeById(Guid id) => DHCPv4Scopes.ContainsKey(id) == false ? GetDefaultScopeDHCPv4(id) : DHCPv4Scopes[id];

        private Dictionary<Guid, Boolean> _scopesToShowForLeases;

        internal void SetLeaseFilter(Dictionary<Guid, Boolean> filteredScopes) => _scopesToShowForLeases = filteredScopes;
    }
}
