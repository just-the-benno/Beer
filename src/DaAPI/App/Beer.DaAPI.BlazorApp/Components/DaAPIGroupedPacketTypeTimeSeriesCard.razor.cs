using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Components
{
    public partial class DaAPIGroupedPacketTypeTimeSeriesCard
    {
        private static readonly List<(String Caption, DHCPv6PacketTypes Value)> _dhcpv6Groupings;
        private static readonly List<(String Caption, DHCPv4MessagesTypes Value)> _dhcpv4Groupings;

        private DateRange _dateRange = new(DateTime.Now.AddDays(-20).Date, DateTime.Now.Date);
        private readonly List<Double> _data = new();
        private readonly List<String> _labels = new();
        private Boolean _firstTime = true;
        private Object _loadingObject = null;

        private DHCPv4MessagesTypes _currentDHCPv4MessageGrouping = DHCPv4MessagesTypes.Discover;
        private DHCPv6PacketTypes _currentDHCPv6MessageGrouping = DHCPv6PacketTypes.Solicit;

        [Parameter] public String Title { get; set; }
        [Parameter] public Boolean IsDHCPv6 { get; set; }
        [Parameter] public DateTime? StartDate { get; set; }
        [Parameter] public DateTime? EndDate { get; set; }

        [Parameter] public Func<DateTime?, DateTime?, DHCPv6PacketTypes, Task<IDictionary<Int32, Int32>>> DHCPv6DataSetLoader { get; set; }
        [Parameter] public Func<DateTime?, DateTime?, DHCPv4MessagesTypes, Task<IDictionary<Int32, Int32>>> DHCPv4DataSetLoader { get; set; }

        static DaAPIGroupedPacketTypeTimeSeriesCard()
        {
            _dhcpv6Groupings = new List<(String display, DHCPv6PacketTypes value)>
            {
                    ("Solicit",DHCPv6PacketTypes.Solicit),
                    ("Request",DHCPv6PacketTypes.REQUEST),
                    ("Renew",DHCPv6PacketTypes.RENEW),
                    ("Rebind",DHCPv6PacketTypes.REBIND),
                    ("Decline",DHCPv6PacketTypes.DECLINE),
                    ("Relase",DHCPv6PacketTypes.RELEASE),
                    ("Confirm",DHCPv6PacketTypes.CONFIRM),
            };

            _dhcpv4Groupings = new List<(String display, DHCPv4MessagesTypes value)>
            {
                    ("Discover",DHCPv4MessagesTypes.Discover),
                    ("Request",DHCPv4MessagesTypes.Request),
                    ("Release",DHCPv4MessagesTypes.Release),
                    ("Decline",DHCPv4MessagesTypes.Decline),
                    ("Inform",DHCPv4MessagesTypes.Inform),
            };
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (_firstTime == true)
            {
                _firstTime = false;
                await LoadDataSets();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataSets();
        }

        private String GetCurrentGroupingLabel() => IsDHCPv6 == true ? _currentDHCPv6MessageGrouping.ToString() : _currentDHCPv4MessageGrouping.ToString();

        private void ClearChart()
        {
            _loadingObject = null;

            _data.Clear();
            _labels.Clear();
        }

        private async Task OnDateRangeChanged(DateRange range)
        {
            _dateRange = range;
            StartDate = range.Start;
            EndDate = range.End;

            await LoadDataSets();
        }

        private async Task LoadDataSets()
        {
            ClearChart();

            var result = IsDHCPv6 == true ?
                await DHCPv6DataSetLoader(StartDate, EndDate, _currentDHCPv6MessageGrouping) :
                await DHCPv4DataSetLoader(StartDate, EndDate, _currentDHCPv4MessageGrouping);

            var dhcpv6Errors = _responseCodeHelper.GetDHCPv6ResponseCodesMapper();
            var dhcpv4Errors = _responseCodeHelper.GetDHCPv4ResponseCodesMapper();

            var dict = IsDHCPv6 == true ? dhcpv6Errors[_currentDHCPv6MessageGrouping] : dhcpv4Errors[_currentDHCPv4MessageGrouping];

            foreach (var item in result)
            {
                String label = String.Empty;
                if(dict.ContainsKey(item.Key) == true)
                {
                    label = dict[item.Key].Name;
                }

                _labels.Add(label);
                _data.Add(item.Value);
            }

            _loadingObject = new object();

        }
    }
}
