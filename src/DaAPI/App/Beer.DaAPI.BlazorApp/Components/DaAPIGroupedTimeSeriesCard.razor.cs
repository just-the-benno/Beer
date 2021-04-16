using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.StatisticsControllerRequests.V1;

namespace Beer.DaAPI.BlazorApp.Components
{
    public class DataSeriesDescription
    {
        public Func<DateTime?, DateTime?, GroupStatisticsResultBy, Task<IDictionary<DateTime, Int32>>> DatasetLoader { get; init; }
        public String Name { get; init; }
        public IDictionary<DateTime, Int32> Recent { get; init; }
    }

    public partial class DaAPIGroupedTimeSeriesCard : ComponentBase
    {
        private record ChartGroupingInfo(String Caption, GroupStatisticsResultBy? Value);

        private readonly List<ChartSeries> _series = new();
        private String[] _labels = Array.Empty<String>();
        private readonly ChartOptions _chartOptions = new();

        private List<ChartGroupingInfo> _groupings = new();

        private static readonly Dictionary<GroupStatisticsResultBy?, Func<DateTime, String>> _unitMapper = new()
        {
            { GroupStatisticsResultBy.Day, d => d.ToString("dd.MM.yy") },
            { GroupStatisticsResultBy.Week, d => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString() },
            { GroupStatisticsResultBy.Month, d => d.ToString("MM.yy") },
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadDataSets();
        }

        private String GetCurrentGroupingLabel() => GroupedBy.HasValue == false ? L["RecentCaption"] : _groupings.FirstOrDefault(x => x.Value == GroupedBy.Value)?.Caption; 

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            _groupings = new List<ChartGroupingInfo>
            {
                new ChartGroupingInfo(L["DayCaption"],GroupStatisticsResultBy.Day),
                new ChartGroupingInfo(L["WeekCaption"],GroupStatisticsResultBy.Week),
                new ChartGroupingInfo(L["MonthCaption"],GroupStatisticsResultBy.Month),
            };

            if (HasRecent == true)
            {
                _groupings.Insert(0, new ChartGroupingInfo(L["RecentCaption"], null));
            }
        }

        private void ClearChart()
        {
            _series.Clear();
            _labels = Array.Empty<String>();
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

            Func<DateTime, String> mapper;
            if (GroupedBy.HasValue == false)
            {
                mapper = d => d.ToString("DD.MM.YY hh:mm");
            }
            else
            {
                mapper = _unitMapper[GroupedBy];
            }

            HashSet<DateTime> xValues = new();

            List<IDictionary<DateTime, Int32>> yValueSeries = new();
            
            foreach (var item in DataSeriesDescription ?? Array.Empty<DataSeriesDescription>())
            {
                var dataSeries = GroupedBy.HasValue == false ?
                (item.Recent ?? new Dictionary<DateTime,Int32>() ) :
                await item.DatasetLoader(StartDate, EndDate, GroupedBy.Value);

                foreach (var point in dataSeries)
                {
                    xValues.Add(point.Key);
                }

                yValueSeries.Add(dataSeries);
            }

            foreach (var xValue in xValues)
            {
                foreach (var series in yValueSeries)
                {
                    if(series.ContainsKey(xValue) == false)
                    {
                        series.Add(xValue, 0);
                    }
                }
            }

            _labels = xValues.OrderBy(x => x).Select(x => mapper(x)).ToArray();

            for (int i = 0; i < yValueSeries.Count; i++)
            {
                _series.Add(new ChartSeries
                {
                    Data = yValueSeries[i].OrderBy(x => x.Key).Select(x => (Double)x.Value).ToArray(),
                    Name = DataSeriesDescription.ElementAt(i).Name,
                });
            }

            Double max = _series.Select(x => x.Data.Max()).Max();

            Double tick = GetNearestTickValue(max,10);
            _chartOptions.YAxisTicks = (Int32)tick;
        }

        private static Double GetNearestTickValue(double initialDelta, Int32 initialNumberOfSteps)
        {
            Int32 scalingFactor = 0;
            Double valuePerTick = initialDelta / (initialNumberOfSteps - 1);

            if (initialDelta > 1)
            {
                while (valuePerTick > 1)
                {
                    valuePerTick /= 10;
                    scalingFactor++;
                }
            }
            else
            {
                while (valuePerTick < 0.1)
                {
                    valuePerTick *= 10;
                    scalingFactor++;
                }
            }

            if (valuePerTick < 0.15)
            {
                valuePerTick = 0.1;
            }
            else if (valuePerTick < 0.35)
            {
                valuePerTick = 0.2;
            }
            else
            {
                valuePerTick = 0.5;
            }

            if (initialDelta > 1)
            {
                for (int i = 0; i < scalingFactor; i++)
                {
                    valuePerTick *= 10;
                }
            }
            else
            {
                for (int i = 0; i < scalingFactor; i++)
                {
                    valuePerTick /= 10;
                }
            }

            return valuePerTick;
        }
    }
}
