using System;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using Humanizer;
using MudBlazor;
using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Common;

namespace Beer.DaAPI.BlazorApp.Pages.Dashboard
{
    public partial class DashboardPage
    {
        private readonly DashboardPageViewModel _vm = new();
        private String _activeLeaseSearchterm = String.Empty;


        private String GetPacketHandledError(IPacketEntry entry) =>
         entry switch
         {
             DHCPv6PacketHandledEntryViewModel vm => _responseCodeHelper.GetErrorName(vm.RequestType, entry.ErrorCode),
             DHCPv4PacketHandledEntryViewModel vm2 => _responseCodeHelper.GetErrorName(vm2.RequestType, entry.ErrorCode),
             _ => string.Empty
         };

        private static String GetScopeLink(ILeaseEntry entry)
        {
            if (entry is DHCPv6LeaseEntryViewModel)
            {
                return $"/scopes/dhcpv6/details/{entry.ScopeId}";
            }
            else
            {
                return $"/scopes/dhcpv4/details/{entry.ScopeId}";
            }
        }

        private static String GetScopeName(ILeaseEntry entry) => entry switch
        {
            DHCPv6LeaseEntryViewModel castedEntry => castedEntry.Scope?.Name ?? String.Empty,
            DHCPv4LeaseEntryViewModel castedEntry => castedEntry.Scope?.Name ?? String.Empty,
            _ => String.Empty,
        };

        private void OpenPacketDetailsDialog(IPacketEntry entry, Boolean isResponse)
        {
            if (entry is DHCPv6PacketHandledEntry dhcpv6Entry)
            {
                DHCPv6Packet packet = DHCPv6Packet.FromByteArray(isResponse == false ? dhcpv6Entry.Request.Content : dhcpv6Entry.Response.Content,
                    new IPv6HeaderInformation(
                        IPv6Address.FromString(isResponse == false ? dhcpv6Entry.Request.Header.Source : dhcpv6Entry.Response.Header.Source),
                        IPv6Address.FromString(isResponse == false ? dhcpv6Entry.Request.Header.Destination : dhcpv6Entry.Response.Header.Destination)
                        ));

                DialogParameters parameters = new();
                parameters.Add(nameof(DHCPv6PacketDetailsDialog.Packet), packet);

                _dialogService.Show<DHCPv6PacketDetailsDialog>(L["DHCPv6PacketDetailsDialogTitle"], parameters, 
                    new DialogOptions() { MaxWidth = MaxWidth.Large, FullWidth = false });
            }
            else if(entry is DHCPv4PacketHandledEntry dhcpv4Entry)
            {
                DHCPv4Packet packet = DHCPv4Packet.FromByteArray(isResponse == false ? dhcpv4Entry.Request.Content : dhcpv4Entry.Response.Content,
                   new IPv4HeaderInformation(
                       IPv4Address.FromString(isResponse == false ? dhcpv4Entry.Request.Header.Source : dhcpv4Entry.Response.Header.Source),
                       IPv4Address.FromString(isResponse == false ? dhcpv4Entry.Request.Header.Destination : dhcpv4Entry.Response.Header.Destination)
                       ));

                DialogParameters parameters = new();
                parameters.Add(nameof(DHCPv4PacketDetailsDialog.Packet), packet);

                _dialogService.Show<DHCPv4PacketDetailsDialog>(L["DHCPv4PacketDetailsDialogTitle"], parameters,
                    new DialogOptions() { MaxWidth = MaxWidth.Large, FullWidth = false });
            }
        }

        private String GetTimeRemainingOfLease(ILeaseEntry entry)
        {
            DateTime now = DateTime.Now;
            var diff = entry.End - now;
            if (diff.TotalSeconds < 0)
            {
                return L["LeaseRunningOutOfTime"];
            }

            return diff.Humanize();
        }

        private static String GetAgeOfLease(ILeaseEntry entry) => $"{(DateTime.Now - entry.Start).Humanize()}";

        private static Color GetColorBasedOnLifetime(ILeaseEntry entry) => (entry.End - DateTime.Now) switch
        {
            TimeSpan n when n.TotalSeconds < 0 => Color.Dark,
            TimeSpan n when n.TotalSeconds < 120 => Color.Error,
            TimeSpan n when n.TotalHours < 1 => Color.Warning,
            TimeSpan n when n.TotalHours < 2 => Color.Info,
            _ => Color.Default,
        };

        protected override async Task OnInitializedAsync()
        {
            var dhcpv6Scopes = await _service.GetDHCPv6ScopesAsList();
            var dhcpv4Scopes = await _service.GetDHCPv4ScopesAsList();

            _vm.Response = await _service.GetDashboard<DashboardViewModelResponse>();

            _vm.SetDHCPv6Scopes(dhcpv6Scopes);
            _vm.SetDHCPv4Scopes(dhcpv4Scopes);

            await base.OnInitializedAsync();
        }
    }
}
