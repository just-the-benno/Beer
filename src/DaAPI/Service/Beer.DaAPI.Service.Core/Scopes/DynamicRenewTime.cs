using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Scopes
{
    public record LeaseTimeValues(TimeSpan RenewTime, TimeSpan ReboundTime, TimeSpan Lifespan);

    public class DynamicRenewTime : Value<DynamicRenewTime>
    {
        private static Random _random = new();
        private const Int32 _maxDeltaForVariance = 10 * 60; //10 minutes;
        private const Int32 _minDeltaForVariance = 10;


        public Byte Hour { get; init; }
        public Byte Minutes { get; init; }
       
        public UInt32 MinutesToRebound { get; init; }
        public UInt32 MinutesToEndOfLife { get; init; }

        private DynamicRenewTime()
        {

        }

        public static DynamicRenewTime WithDefaultRange(Int32 hours, Int32 minutes) => WithSpecificRange(hours, minutes, 30, 60);

        public static DynamicRenewTime WithSpecificRange(Int32 hours, Int32 minutes, Int32 minutesToRebound, Int32 minutesToEndOfLife)
        {
            if (hours < 0 || hours > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), "hours needs to be between 0 and 24");
            }

            if (minutes < 0 || minutes > 60)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), "minutes needs to be between 0 and 24");
            }

            if(minutesToRebound < 10 || minutesToRebound > 24 * 60)
            {
                throw new ArgumentOutOfRangeException(nameof(minutesToRebound), "the rebound intervall needs to be between 10 minutes and a day");

            }

            if (minutesToEndOfLife < 15 || minutesToEndOfLife > 24 * 60)
            {
                throw new ArgumentOutOfRangeException(nameof(minutesToEndOfLife), "the lifetime intervall needs to be between 10 minutes and a day");
            }

            if(minutesToRebound > minutesToEndOfLife)
            {
                throw new ArgumentException("rebound time should be smaller than lifetime");
            }

            return new DynamicRenewTime
            {
                Hour = (Byte)hours,
                Minutes = (Byte)minutes,
                MinutesToRebound = (UInt32)minutesToRebound,
                MinutesToEndOfLife = (UInt32)minutesToEndOfLife,
            };
        }

        public LeaseTimeValues GetLeaseTimers()
        {
            DateTime now = DateTime.Now;

            DateTime renewTime = new (now.Year, now.Month, now.Day, Hour, Minutes, 0);

            TimeSpan renewSpan = renewTime - now;
            if(renewSpan.TotalMinutes < 10) 
            {
                renewSpan = renewSpan.Add(TimeSpan.FromHours(24));
            }

            Int32 randomOffset = _random.Next(_minDeltaForVariance, _maxDeltaForVariance); //10 minutes 
            if(_random.NextDouble() > 0.5)
            {
                randomOffset *= -1;
            }

            renewSpan = renewSpan.Add(TimeSpan.FromSeconds(randomOffset));

            return new (renewSpan, renewSpan.Add(TimeSpan.FromMinutes(MinutesToRebound)), renewSpan.Add(TimeSpan.FromMinutes(MinutesToEndOfLife)));
        }
    }
}
