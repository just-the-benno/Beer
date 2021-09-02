using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Shared.Helper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.CommenRequests.V1;
using static Beer.DaAPI.Shared.Responses.CommenResponses.V1;
using static Beer.DaAPI.Shared.Responses.PacketMonitorResponses.V1;

namespace Beer.DaAPI.BlazorApp.Components
{
    public partial class DaAPILeaseEventHistory : IDisposable
    {
        private MudTable<LeaseEventOverview> _table;
        private DaAPILeaseEventHistoryFilterModel _filter;
        private Timer _timer;
        private Int32 _totalItems = 0;
        private Boolean _timerExecutionInProgress = false;

        [Parameter] public Guid? ScopeId { get; set; }
        [Parameter] public Boolean DisplayIPv6Packets { get; set; }

        private void UpdateFilter(DaAPILeaseEventHistoryFilterModel model)
        {
            _filter = model;
            _table.ReloadServerData();
        }

        private String GetScopeLink(LeaseEventOverview entry) =>
            DisplayIPv6Packets == true ? $"/scopes/dhcpv6/details/{entry.Scope.Id}" : $"/scopes/dhcpv4/details/{entry.Scope.Id}";

        protected override void OnInitialized()
        {
            _filter = new DaAPILeaseEventHistoryFilterModel();

            if (_timer == null)
            {
                _timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            }
        }

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

        [Parameter] public Func<FilterLeaseHistoryRequest, Task<FilteredResult<LeaseEventOverview>>> FilterMethod { get; set; }

        private FilterLeaseHistoryRequest GetFilterAsRequest(Int32 amount, Int32 start, DaAPILeaseEventHistoryFilterModel filter)
        {
            return new FilterLeaseHistoryRequest
            {
                Amount = amount,
                Start = start,
                StartTime = SetTime(filter.From, filter.FromTime),
                EndTime = SetTime(filter.To, filter.ToTime),
                ScopeId = ScopeId,
                IncludeChildren = filter.IncludeChildScopes,
                Address = filter.Address,
            };
        }

        private async void TimerCallback(Object _)
        {
            if (_timerExecutionInProgress == true)
            {
                return;
            }

            _timerExecutionInProgress = true;

            var filter = _filter ?? new DaAPILeaseEventHistoryFilterModel();
            var filterRequest = GetFilterAsRequest(0, 0, filter);

			FilteredResult<LeaseEventOverview> result = await FilterMethod.Invoke(filterRequest);

			if (result.Total != _totalItems)
			{
				await _table.ReloadServerData();
			}

			_timerExecutionInProgress = false;
        }

        private async Task<TableData<LeaseEventOverview>> LoadEvents(TableState state)
        {
            var filter = _filter ?? new DaAPILeaseEventHistoryFilterModel();
            var filterRequest = GetFilterAsRequest(state.PageSize, state.Page * state.PageSize, filter);

            FilteredResult<LeaseEventOverview> result = await FilterMethod.Invoke(filterRequest);

            _totalItems = result.Total;

            return new TableData<LeaseEventOverview>
            {
                Items = result.Result,
                TotalItems = _totalItems,
            };
        }

        [Parameter] public Func<Guid, Boolean, Task<PacketInfo>> PacketRequestLoader { get; set; }

        private async Task OpenPacketDetailsDialog(LeaseEventOverview entry, Boolean isResponse)
        {
            var dataPacketInfo = await PacketRequestLoader(entry.PacketHandledId, isResponse);

            if (DisplayIPv6Packets == true)
            {
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
            else
            {
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
