using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Shared.Helper;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Commands.PacketMonitorRequest.V1;
using static Beer.DaAPI.Shared.Responses.PacketMonitorResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.PacketMonitor
{
    public partial class PacketMonitorPage : IDisposable
    {
        private MudTable<IPacketOverview> _table;
        private PacketFilterModel _filter;
        private Timer _timer;
        private Int32 _totalItems = 0;
        private Boolean _timerExecutionInProgress = false;

        private String GetPacketHandledError(IPacketOverview entry) =>
             entry switch
             {
                 DHCPv6PacketOverview vm => ResponseCodeHelper.GetErrorName(vm.RequestMessageType, entry.ResultCode),
                 DHCPv4PacketOverview vm2 => ResponseCodeHelper.GetErrorName(vm2.RequestMessageType, entry.ResultCode),
                 _ => string.Empty
             };

        private String GetScopeLink(IPacketOverview entry) =>
            entry switch
            {
                DHCPv6PacketOverview => $"/scopes/dhcpv6/details/{entry.Scope.Id}",
                DHCPv4PacketOverview => $"/scopes/dhcpv4/details/{entry.Scope.Id}",
                _ => "",
            };

        private void UpdateFilter(PacketFilterModel model)
        {
            _filter = model;
            _table.ReloadServerData();
           
        }

        protected override void OnInitialized()
        {
            if (_timer == null)
            {
                _timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            }
        }

        private async void TimerCallback(Object _)
        {
            if(_timerExecutionInProgress == true)
            {
                return;
            }

            _timerExecutionInProgress = true;

            var filter = _filter ?? new PacketFilterModel();

            DHCPv4PacketFilter dhcpv4PacketFilter = GetDHCPv4Filter(0, 0, filter);
            DHCPv6PacketFilter dhcpv6PacketFilter = GetDHCPv6Filter(0, 0, filter);

            FilteredResult<DHCPv4PacketOverview> dhcpv4Result = filter.FilterMode == PacketFilterMode.OnlyIPv6 ? new FilteredResult<DHCPv4PacketOverview>() : await Service.GetGetDHCPv4PacketFromMonitor(dhcpv4PacketFilter);
            FilteredResult<DHCPv6PacketOverview> dhcpv6Result = filter.FilterMode == PacketFilterMode.OnlyIPv4 ? new FilteredResult<DHCPv6PacketOverview>() : await Service.GetGetDHCPv6PacketFromMonitor(dhcpv6PacketFilter);

            Int32 total = dhcpv4Result.Total + dhcpv6Result.Total;
            if (total != _totalItems)
            {
                await _table.ReloadServerData();
            }

            _timerExecutionInProgress = false;
        }

        private bool HasResponseType(IPacketOverview entry) => entry switch
        {
            DHCPv6PacketOverview vm => vm.ResponseMessageType.HasValue,
            DHCPv4PacketOverview vm2 => vm2.ResponseMessageType.HasValue,
            _ => false,
        };

        private String GetResponseType(IPacketOverview entry) => entry switch
        {
            DHCPv6PacketOverview vm => vm.ResponseMessageType.ToString(),
            DHCPv4PacketOverview vm2 => vm2.ResponseMessageType.ToString(),
            _ => String.Empty,
        };


        private DateTime? SetTime(DateTime? date, TimeSpan? time)
        {
            var result = new DateTime?();
            if (date.HasValue == false && time.HasValue == true)
            {
                result = DateTime.Today.AddMinutes(time.Value.TotalMinutes);
            }
            else if (date.HasValue == true && time.HasValue == true)
            {
                result = date.Value.AddMinutes(time.Value.TotalMinutes);
            }

            return result;
        }


        private async Task<TableData<IPacketOverview>> LoadPackets(TableState state)
        {
            var filter = _filter ?? new PacketFilterModel();

            DHCPv4PacketFilter dhcpv4PacketFilter = GetDHCPv4Filter(
                state.PageSize, state.Page * state.PageSize, filter);

            DHCPv6PacketFilter dhcpv6PacketFilter = GetDHCPv6Filter(
                state.PageSize, state.Page * state.PageSize, filter);

            FilteredResult<DHCPv4PacketOverview> dhcpv4Result = filter.FilterMode == PacketFilterMode.OnlyIPv6 ? new FilteredResult<DHCPv4PacketOverview>() : await Service.GetGetDHCPv4PacketFromMonitor(dhcpv4PacketFilter);
            FilteredResult<DHCPv6PacketOverview> dhcpv6Result = filter.FilterMode == PacketFilterMode.OnlyIPv4 ? new FilteredResult<DHCPv6PacketOverview>() : await Service.GetGetDHCPv6PacketFromMonitor(dhcpv6PacketFilter);

            _totalItems = dhcpv4Result.Total + dhcpv6Result.Total;

            return new TableData<IPacketOverview>
            {
                Items = (dhcpv4Result.Result as IEnumerable<IPacketOverview>).Union(dhcpv6Result.Result).OrderByDescending(x => x.Timestamp).Take(state.PageSize).ToArray(),
                TotalItems = _totalItems,
            };
        }

        private DHCPv6PacketFilter GetDHCPv6Filter(Int32 amount, Int32 start, PacketFilterModel filter)
        {
            return new DHCPv6PacketFilter
            {
                Amount = amount,
                Start = start,

                From = SetTime(filter.From, filter.FromTime),
                To = SetTime(filter.To, filter.ToTime),
                MacAddress = filter.MacAddress,
                RequestedIp = filter.RequestedIp,
                RequestedPrefix = filter.RequestedPrefix,
                LeasedIp = filter.LeasedIp,
                LeasedPrefix = filter.LeasedPrefix,
                Filtered = filter.Filtered,
                Invalid = filter.Invalid,
                SourceAddress = filter.SourceAddress,
                DestinationAddress = filter.DestinationAddress,
                ScopeId = filter.ScopeId,
                IncludeScopeChildren = filter.IncludeScopeChildren,
                RequestMessageType = filter.DHCPv6RequestMessageType,
                ResponseMessageType = filter.DHCPv6ResponseMessageType,
                HasAnswer = filter.HasAnswer,
                ResultCode = filter.ResultCode,
            };
        }

        private DHCPv4PacketFilter GetDHCPv4Filter(Int32 amount, Int32 start, PacketFilterModel filter)
        {
            return new DHCPv4PacketFilter
            {
                Amount = amount,
                Start = start,
                From = SetTime(filter.From, filter.FromTime),
                To = SetTime(filter.To, filter.ToTime),
                MacAddress = filter.MacAddress,
                RequestedIp = filter.RequestedIp,
                LeasedIp = filter.LeasedIp,
                Filtered = filter.Filtered,
                Invalid = filter.Invalid,
                SourceAddress = filter.SourceAddress,
                DestinationAddress = filter.DestinationAddress,
                ScopeId = filter.ScopeId,
                IncludeScopeChildren = filter.IncludeScopeChildren,
                RequestMessageType = filter.DHCPv4RequestMessageType,
                ResponseMessageType = filter.DHCPv4ResponseMessageType,
                HasAnswer = filter.HasAnswer,
                ResultCode = filter.ResultCode,
            };
        }

        private async Task OpenPacketDetailsDialog(IPacketOverview entry, Boolean isResponse)
        {
            if (entry is DHCPv6PacketOverview)
            {
                var dataPacketInfo = isResponse == false ? await Service.GetDHCPv6PacketRequest(entry.Id) : await Service.GetDHCPv6PacketResponse(entry.Id);

                DHCPv6Packet packet = DHCPv6Packet.FromByteArray(dataPacketInfo.Content,
                    new IPv6HeaderInformation(
                        IPv6Address.FromString(dataPacketInfo.Source),
                        IPv6Address.FromString(dataPacketInfo.Destination)
                        ));

                DialogParameters parameters = new();
                parameters.Add(nameof(DHCPv6PacketDetailsDialog.Packet), packet);

                DialogService.Show<DHCPv6PacketDetailsDialog>(L["DHCPv6PacketDetailsDialogTitle"], parameters,
                    new DialogOptions() { MaxWidth = MaxWidth.Large, FullWidth = false });
            }
            else if (entry is DHCPv4PacketOverview)
            {
                var dataPacketInfo = isResponse == false ? await Service.GetDHCPv4PacketRequest(entry.Id) : await Service.GetDHCPv4PacketResponse(entry.Id);

                DHCPv4Packet packet = DHCPv4Packet.FromByteArray(dataPacketInfo.Content,
                   new IPv4HeaderInformation(
                       IPv4Address.FromString(dataPacketInfo.Source),
                       IPv4Address.FromString(dataPacketInfo.Destination)
                       ));

                DialogParameters parameters = new();
                parameters.Add(nameof(DHCPv4PacketDetailsDialog.Packet), packet);

                DialogService.Show<DHCPv4PacketDetailsDialog>(L["DHCPv4PacketDetailsDialogTitle"], parameters,
                    new DialogOptions() { MaxWidth = MaxWidth.Large, FullWidth = false });
            }
        }

        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }
    }
}
