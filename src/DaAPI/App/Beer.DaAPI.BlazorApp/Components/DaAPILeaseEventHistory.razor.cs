using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Shared.Helper;
using Beer.DaAPI.Shared.JsonConverters;
using Humanizer;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4LeaseEvents;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6LeaseEvents;
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

        private JsonSerializerSettings _settings;

        public DaAPILeaseEventHistory()
        {
            _settings = new JsonSerializerSettings();
            _settings.LoadCustomerConverters();
        }

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

            var filter = _filter ?? new DaAPILeaseEventHistoryFilterModel { IncludeChildScopes = true, };
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

        private String GetFriendlyEventName(String eventName) => L[$"FriendlyName_{eventName}"];

        private IEnumerable<(String Property, String Value)> GetEventProperties(LeaseEventOverview leaseEventOverview)
        {
            var type = Type.GetType(leaseEventOverview.EventType + $", {Assembly.GetAssembly(typeof(ByteHelper)).FullName}");

            var @event = JsonConvert.DeserializeObject(
                leaseEventOverview.EventData,
                type,
                _settings
                );

            List<(String Property, String Value)> result = new();

            switch (@event)
            {
                case DHCPv4AddressSuspendedEvent e:
                    result.Add((L["DHCPv4AddressSuspendedEvent_Property_Address"], e.Address.ToString()));
                    result.Add((L["DHCPv4AddressSuspendedEvent_Property_SuspendedTill"], e.SuspendedTill.ToString("dd.MM.yy HH:mm")));
                    break;

                case DHCPv4LeaseCanceledEvent e:
                    result.Add((L["DHCPv4LeaseCanceledEvent_Property_Reason"], L[$"DHCPv4LeaseCanceledEvent_Property_Reasons_{e.Reason}"]));
                    result.Add((L["DHCPv4LeaseCanceledEvent_Property_ForceRemove"], L[$"{e.ForceRemove}"]));
                    break;

                case DHCPv4LeaseCreatedEvent e:
                    result.Add((L["DHCPv4LeaseCreatedEvent_Property_Address"], e.Address.ToString()));
                    try
                    {
                        String value = "";
                        var identifier = DHCPv4ClientIdentifier.FromOptionData(e.ClientIdenfier);
                        if (identifier.DUID != null)
                        {
                            if (identifier.DUID is LinkLayerAddressAndTimeDUID llatDuid)
                            {
                                value = ByteHelper.ToString(llatDuid.LinkLayerAddress, ':', false);
                            }
                            else if (identifier.DUID is LinkLayerAddressDUID llDuid)
                            {
                                value = ByteHelper.ToString(llDuid.LinkLayerAddress, ':', false);
                            }
                            else
                            {
                                value = ByteHelper.ToString(e.ClientIdenfier, ':', false);
                            }
                        }
                        else if (identifier.HasHardwareAddress() == false)
                        {
                            value = ByteHelper.ToString(identifier.HwAddress, ':', false);
                        }
                        else
                        {
                            value = $"{identifier.IdentifierValue} (${ByteHelper.ToString(e.ClientMacAddress, ':', false)})";
                        }

                        result.Add((L["DHCPv4LeaseCreatedEvent_Property_ClientIdenfier"], value));
                    }
                    catch (Exception)
                    {
                    }
                    result.Add((L["DHCPv4LeaseCreatedEvent_Property_StartedAt"], e.StartedAt.ToString("dd.MM.yy HH:mm")));
                    result.Add((L["DHCPv4LeaseCreatedEvent_Property_ValidUntil"], e.ValidUntil.ToString("dd.MM.yy HH:mm")));
                    result.Add((L["DHCPv4LeaseCreatedEvent_Property_RenewalTime"], e.RenewalTime.Humanize()));
                    result.Add((L["DHCPv4LeaseCreatedEvent_Property_PreferredLifetime"], e.PreferredLifetime.Humanize()));
                    break;

                case DHCPv4LeaseRenewedEvent e:
                    result.Add((L["DHCPv4LeaseRenewedEvent_Property_RenewSpan"], e.RenewSpan.Humanize()));
                    result.Add((L["DHCPv4LeaseRenewedEvent_Property_ReboundSpan"], e.ReboundSpan.Humanize()));
                    result.Add((L["DHCPv4LeaseRenewedEvent_Property_Reset"], L[$"{e.Reset}"]));
                    result.Add((L["DHCPv4LeaseRenewedEvent_Property_End"], e.End.ToString("dd.MM.yy HH:mm")));
                    break;

                case DHCPv6AddressSuspendedEvent e:
                    result.Add((L["DHCPv6AddressSuspendedEvent_Property_Address"], e.Address.ToString()));
                    result.Add((L["DHCPv6AddressSuspendedEvent_Property_SuspendedTill"], e.SuspendedTill.ToString("dd.MM.yy HH:mm")));
                    break;

                case DHCPv6LeasePrefixAddedEvent e:
                    result.Add((L["DHCPv6LeasePrefixAddedEvent_Property_Prefix"], e.NetworkAddress.ToString()));
                    result.Add((L["DHCPv6LeasePrefixAddedEvent_Property_PrefixLength"], e.PrefixLength.ToString()));
                    result.Add((L["DHCPv6LeasePrefixAddedEvent_Property_Started"], e.Started.ToString("dd.MM.yy HH:mm")));
                    result.Add((L["DHCPv6LeasePrefixAddedEvent_Property_PrefixAssociationId"], e.PrefixAssociationId.ToString()));
                    break;

                case DHCPv6LeasePrefixActvatedEvent e:
                    result.Add((L["DHCPv6LeasePrefixActvatedEvent_Property_Prefix"], e.NetworkAddress.ToString()));
                    result.Add((L["DHCPv6LeasePrefixActvatedEvent_Property_PrefixLength"], e.PrefixLength.ToString()));
                    result.Add((L["DHCPv6LeasePrefixActvatedEvent_Property_PrefixAssociationId"], e.PrefixAssociationId.ToString()));
                    break;
                
                case DHCPv6LeaseCanceledEvent e:
                    result.Add((L["DHCPv6LeaseCanceledEvent_Property_Reason"], L[$"DHCPv4LeaseCanceledEvent_Property_Reasons_{e.Reason}"]));
                    break;

                case DHCPv6LeaseCreatedEvent e:
                    result.Add((L["DHCPv6LeaseCreatedEvent_Property_Address"], e.Address.ToString()));
                    result.Add((L["DHCPv6LeaseCreatedEvent_Property_StartedAt"], e.StartedAt.ToString("dd.MM.yy HH:mm")));
                    result.Add((L["DHCPv6LeaseCreatedEvent_Property_ValidUntil"], e.ValidUntil.ToString("dd.MM.yy HH:mm")));
                    result.Add((L["DHCPv6LeaseCreatedEvent_Property_RenewalTime"], e.RenewalTime.Humanize()));
                    result.Add((L["DHCPv6LeaseCreatedEvent_Property_PreferredLifetime"], e.PreferredLifetime.Humanize()));
                    result.Add((L["DHCPv6LeaseCreatedEvent_Property_IdentityAssocationId"], e.IdentityAssocationId.ToString()));
                    if (e.HasPrefixDelegation == true)
                    {
                        result.Add((L["DHCPv6LeaseCreatedEvent_Property_DelegatedNetworkAddress"], e.DelegatedNetworkAddress.ToString()));
                        result.Add((L["DHCPv6LeaseCreatedEvent_Property_PrefixLength"], e.PrefixLength.ToString()));
                        result.Add((L["DHCPv6LeaseCreatedEvent_Property_IdentityAssocationIdForPrefix"], e.IdentityAssocationIdForPrefix.ToString()));
                    }
                    break;

                case DHCPv6LeaseReleasedEvent e:
                    result.Add((L["DHCPv6LeaseReleasedEvent_Property_OnlyPrefix"], L[$"{e.OnlyPrefix}"]));
                    break;

                case DHCPv6LeaseRenewedEvent e:
                    result.Add((L["DHCPv6LeaseRenewedEvent_Property_End"], e.End.ToString("dd.MM.yy HH:mm")));
                    result.Add((L["DHCPv6LeaseRenewedEvent_Property_Reset"], L[$"{e.Reset}"]));
                    result.Add((L["DHCPv6LeaseRenewedEvent_Property_ResetPrefix"], L[$"{e.ResetPrefix}"]));
                    result.Add((L["DHCPv6LeaseRenewedEvent_Property_ReboundSpan"], e.ReboundSpan.Humanize()));
                    result.Add((L["DHCPv6LeaseRenewedEvent_Property_RenewSpan"], e.RenewSpan.Humanize()));
                    break;
                default:
                    break;
            }


            return result;
        }

        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }
    }
}
