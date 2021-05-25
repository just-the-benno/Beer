using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Components
{
    public partial class DaAPITimeSpanPicker : MudPicker<TimeSpan?>
    {
        public enum TimeSpanUnits
        {
            Days = 50,
            Hours = 40,
            Minutes = 30,
            Seconds = 20,
        }

        public DaAPITimeSpanPicker() : base(new DefaultConverter<TimeSpan?>())
        {
            Converter.GetFunc = OnGet;
            Converter.SetFunc = OnSet;
        }

        private readonly List<TimeSpanUnits> _allUnits = new List<TimeSpanUnits> { TimeSpanUnits.Days, TimeSpanUnits.Hours, TimeSpanUnits.Minutes, TimeSpanUnits.Seconds };
        private readonly Int32[] _unitAsInts = new[] { (Int32)TimeSpanUnits.Days, (Int32)TimeSpanUnits.Hours, (Int32)TimeSpanUnits.Minutes, (Int32)TimeSpanUnits.Seconds };

        private IList<TimeSpanUnits> GetFields() => _unitAsInts.Where(x => x <= (Int32)MaxUnit && x >= (Int32)MinUnit).Select(x => (TimeSpanUnits)x).ToList();

        [Parameter] public TimeSpan Maximum { get; set; } = TimeSpan.MaxValue;
        [Parameter] public TimeSpan Minimum { get; set; } = TimeSpan.Zero;

        [Parameter] public TimeSpanUnits MinUnit { get; set; } = TimeSpanUnits.Seconds;
        [Parameter] public TimeSpanUnits MaxUnit { get; set; } = TimeSpanUnits.Days;

        private string OnSet(TimeSpan? time) => time.HasValue == false ? String.Empty : time.Value.ToString();

        private TimeSpan? OnGet(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            if (TimeSpan.TryParse(value, out var time))
            {
                return time;
            }

            return null;
        }

        public void Increase(TimeSpanUnits unit)
        {
            if (TimeIntermediate == null)
            {
                TimeIntermediate = TimeSpan.Zero;
            }

            Int32[] representation = new[] { TimeIntermediate.Value.Days, TimeIntermediate.Value.Hours, TimeIntermediate.Value.Minutes, TimeIntermediate.Value.Seconds };
            Int32[] maxValues = new[] { TimeSpan.MaxValue.Days, 24, 60, 60, };
            Int32 startIndex = _allUnits.IndexOf(unit);

            for (int i = startIndex; i >= 0; i--)
            {
                Int32 newValue = representation[i] + 1;

                if (i == 0)
                {
                    if (newValue > maxValues[0])
                    {
                        representation[0] = maxValues[0];
                    }
                    else
                    {
                        representation[0] = newValue;
                    }

                    break;
                }

                if (newValue > maxValues[i])
                {
                    representation[i] = 0;
                }
                else
                {
                    representation[i] = newValue;
                    break;
                }
            }

            var newTimespan = new TimeSpan(representation[0], representation[1], representation[2], representation[3]);
            if (newTimespan <= Maximum)
            {
                TimeIntermediate = newTimespan;
            }
        }

        private EventCallback<MouseEventArgs> GetIncreaseCallback(TimeSpanUnits unit) => EventCallback.Factory.Create<MouseEventArgs>(this, () => Increase(unit));
        private EventCallback<MouseEventArgs> GetDecreaseCallback(TimeSpanUnits unit) => EventCallback.Factory.Create<MouseEventArgs>(this, () => Decrease(unit));

        private Timer _timer;

        private void StartCounter(TimeSpanUnits unit, Action<TimeSpanUnits> invoker)
        {
            if (_timer != null)
            {
                return;
            }

            _timer = new Timer((unused) =>
            {
                InvokeAsync(() =>
                {
                    invoker(unit);
                    StateHasChanged();
                });

            }, new object(), 0, 70);
        }

        private EventCallback<MouseEventArgs> GetStartIncreaseTimerCallback(TimeSpanUnits unit) => EventCallback.Factory.Create<MouseEventArgs>(this, () => StartIncreaseCounter(unit));
        private EventCallback<MouseEventArgs> GetStartDecreaseTimerCallback(TimeSpanUnits unit) => EventCallback.Factory.Create<MouseEventArgs>(this, () => StartDecreaseCounter(unit));

        private void StartIncreaseCounter(TimeSpanUnits unit) => StartCounter(unit, Increase);
        private void StartDecreaseCounter(TimeSpanUnits unit) => StartCounter(unit, Decrease);

        private void StopCouter()
        {
            _timer.Dispose();
            _timer = null;
        }

        public void Decrease(TimeSpanUnits unit)
        {
            if (TimeIntermediate == null)
            {
                TimeIntermediate = TimeSpan.Zero;
            }

            Int32[] representation = new[] { TimeIntermediate.Value.Days, TimeIntermediate.Value.Hours, TimeIntermediate.Value.Minutes, TimeIntermediate.Value.Seconds };
            Int32[] maxValues = new[] { TimeSpan.MinValue.Days, 24, 60, 60, };
            Int32 startIndex = _allUnits.IndexOf(unit);

            for (int i = startIndex; i >= 0; i--)
            {
                Int32 newValue = representation[i] - 1;

                if (i == 0)
                {
                    if (newValue < maxValues[0])
                    {
                        representation[0] = maxValues[0];
                    }
                    else
                    {
                        representation[i] = newValue;
                    }

                    break;
                }

                if (newValue < 0)
                {
                    representation[i] = maxValues[i] - 1;
                    if (representation[i] == Int32.MinValue)
                    {
                        representation[i] = Int32.MinValue + 1;
                    }
                }
                else
                {
                    representation[i] = newValue;
                    break;
                }
            }

            var newTimespan = new TimeSpan(representation[0], representation[1], representation[2], representation[3]);
            if (newTimespan >= Minimum)
            {
                TimeIntermediate = newTimespan;
            }
        }

        public Int32 GetTimeSpanValue(TimeSpanUnits unit) => unit switch
        {
            TimeSpanUnits.Days => TimeIntermediate?.Days ?? 0,
            TimeSpanUnits.Hours => TimeIntermediate?.Hours ?? 0,
            TimeSpanUnits.Minutes => TimeIntermediate?.Minutes ?? 0,
            TimeSpanUnits.Seconds => TimeIntermediate?.Seconds ?? 0,
            _ => 0,
        };

        public static String GetTimeUnitDisplay(TimeSpanUnits unit) => unit switch
        {
            TimeSpanUnits.Days => "d",
            TimeSpanUnits.Hours => "h",
            TimeSpanUnits.Minutes => "m",
            TimeSpanUnits.Seconds => "s",
            _ => String.Empty,
        };

        internal TimeSpan? TimeIntermediate { get; private set; }

        /// <summary>
        /// The currently selected time (two-way bindable). If null, then nothing was selected.
        /// </summary>
        [Parameter]
        public TimeSpan? Time
        {
            get => _value;
            set => SetTimeAsync(value, true).AndForget();
        }

        protected async Task SetTimeAsync(TimeSpan? time, bool updateValue)
        {
            if (_value != time)
            {
                TimeIntermediate = time;
                _value = time;
                if (updateValue)
                    await SetTextAsync(Converter.Set(_value), false);
                await TimeChanged.InvokeAsync(_value);
                BeginValidate();
            }
        }

        /// <summary>
        /// Fired when the date changes.
        /// </summary>
        [Parameter] public EventCallback<TimeSpan?> TimeChanged { get; set; }

        protected override Task StringValueChanged(string value)
        {
            Touched = true;
            // Update the time property (without updating back the Value property)
            return SetTimeAsync(Converter.Get(value), false);
        }

        protected override void Submit()
        {
            Time = TimeIntermediate;
        }

        public override void Clear(bool close = true)
        {
            Time = null;
            TimeIntermediate = null;
            base.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                _timer?.Dispose();
            }
        }
    }
}
