using Beer.DaAPI.BlazorApp.Pages.Dashboard;
using Beer.DaAPI.BlazorApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.CommenResponses.V1;
using static Beer.DaAPI.Shared.Responses.PacketMonitorResponses.V1;

namespace Beer.DaAPI.BlazorApp.Components
{
    public partial class DaAPIDHCPLeaseTable<TLease> where TLease : ILeaseOverview
    {
        [Parameter] public Guid ScopeId { get; set; }
        [Parameter] public Boolean IncludeChildren { get; set; } = false;
        [Parameter] public Func<Guid, Boolean, DateTime?, Task<IEnumerable<TLease>>> LeaseGetter { get; set; }
        [Parameter] public Func<Guid, Task<Boolean>> LeaseDeleter { get; set; }
        [Parameter] public Func<Guid, String> ScopeLinkGetter { get; set; }
        [Parameter] public RenderFragment<TLease> AddressContent { get; set; }
        [Parameter] public RenderFragment<TLease> ClientIdentifierContent { get; set; }

        [Inject] DaAPIService Service { get; set; }
        [Inject] ISnackbar SnackBarService { get; set; }

        private IDictionary<PacketStatisticTimePeriod, IncomingAndOutgoingPacketStatisticItem> _packetStatistics;
        private Int32 _activeLeasesAmount;
        private Int32 _reboundingLeasesAmount;

        private Boolean _enableHistory;

        private const Int32 _itemsPerPage = 50;

        public Int32 PageIndex { get; set; } = 1;

        private DHCPLeaseStates GetState(TLease entry)
        {
            if (entry.State == Core.Scopes.LeaseStates.Pending)
            {
                return DHCPLeaseStates.Pending;
            }
            else
            {
                var referenceDate = PointOfView ?? DateTime.UtcNow;

                if (referenceDate >= entry.ReboundTime)
                {
                    return DHCPLeaseStates.Rebinding;
                }
                else if (referenceDate >= entry.RenewTime)
                {
                    return DHCPLeaseStates.Renewing;
                }
                else
                {
                    return DHCPLeaseStates.Active;
                }
            }
        }

        public Boolean EnableHistory
        {
            get => _enableHistory;
            set
            {
                if (value != _enableHistory)
                {
                    _enableHistory = value;
                    if (value == true && _referenceTimeSpan.HasValue == false)
                    {
                        _referenceTimeSpan = DateTime.UtcNow.TimeOfDay;
                        _referenceDate = DateTime.UtcNow.Date;
                    }

                    LoadLeases();
                }
            }
        }

        private TimeSpan? _referenceTimeSpan;

        public TimeSpan? ReferenceTimeSpan
        {
            get => _referenceTimeSpan;
            set
            {
                if (value != _referenceTimeSpan)
                {
                    _referenceTimeSpan = value;

                    LoadLeases();
                }

            }
        }

        private DateTime? _referenceDate;

        public DateTime? ReferenceDate
        {
            get => _referenceDate;
            set
            {
                if (value != _referenceDate)
                {
                    _referenceDate = value;

                    LoadLeases();
                }
            }
        }

        private IncomingAndOutgoingPacketStatisticItem GetHourlyPacketStatistics() => _packetStatistics[PacketStatisticTimePeriod.LastHour];

        private ICollection<TLease> _entries;

        public DateTime? PointOfView { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await LoadLeases();
            await base.OnParametersSetAsync();
        }

        private void Reset(Boolean updateState)
        {
            _entries = null;

            if (updateState == true)
            {
                StateHasChanged();
            }
        }


        private async Task LoadLeases()
        {
            PointOfView = _enableHistory == true ? _referenceDate.Value + _referenceTimeSpan.Value : null;

            _packetStatistics = await Service.GetInAndOutgoingPackets(ScopeId, PointOfView);
            _entries = (await LeaseGetter(ScopeId, IncludeChildren, ReferenceDate)).ToList();

            _activeLeasesAmount = _entries.Count(x => x.State == Core.Scopes.LeaseStates.Active);
            _reboundingLeasesAmount = _entries.Count(x => x.State == Core.Scopes.LeaseStates.Active && PointOfView > x.ReboundTime);
        }

        public async Task CancelLease(TLease lease)
        {
            var result = await LeaseDeleter(lease.Id);

            if (result == true)
            {
                SnackBarService.Add(String.Format(L["CancelLeaseSuccessSnackbarContent"], lease.Address), Severity.Success);

                Reset(true);
                await LoadLeases();
            }
            else
            {
                SnackBarService.Add(String.Format(L["CancelLeaseFailureSnackbarContent"], lease.Address), Severity.Error);
            }
        }

        private async Task ChangeTime(TimeSpan delta)
        {
            DateTime dateTime = (_referenceDate.Value.Date + _referenceTimeSpan.Value).Add(delta);

            _referenceDate = dateTime.Date;
            _referenceTimeSpan = dateTime.TimeOfDay;

            ReferenceDate = dateTime;

            await LoadLeases();
        }

        public async Task GoBackward() => await ChangeTime(-TimeSpan.FromHours(1));
        public async Task GoForward() => await ChangeTime(TimeSpan.FromHours(1));
    }
}
