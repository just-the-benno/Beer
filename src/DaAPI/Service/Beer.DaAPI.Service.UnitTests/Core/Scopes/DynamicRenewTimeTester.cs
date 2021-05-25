using Beer.DaAPI.Core.Scopes;
using Beer.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Beer.DaAPI.UnitTests.Core.Scopes
{
    public class DynamicRenewTimeTester
    {
        [Fact]
        public void DynamicRenewTime_WithDefaultRange()
        {
            var time = DynamicRenewTime.WithDefaultRange(20, 45);

            Assert.Equal(20, time.Hour);
            Assert.Equal(45, time.Minutes);

            Assert.Equal((UInt32)30, time.MinutesToRebound);
            Assert.Equal((UInt32)60, time.MinutesToEndOfLife);
        }

        [Fact]
        public void DynamicRenewTime_WithSpecificRange()
        {
            var time = DynamicRenewTime.WithSpecificRange(20, 45, 15, 25);

            Assert.Equal(20, time.Hour);
            Assert.Equal(45, time.Minutes);

            Assert.Equal((UInt32)15, time.MinutesToRebound);
            Assert.Equal((UInt32)25, time.MinutesToEndOfLife);
        }

        [Theory]
        [InlineData(25)]
        [InlineData(26)]
        [InlineData(-1)]
        public void DynamicRenewTime_Failed_HoursNotInRange(Int32 hourValue)
        {
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => DynamicRenewTime.WithSpecificRange(hourValue, 10, 20, 30));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(61)]
        [InlineData(62)]
        public void DynamicRenewTime_Failed_MinutesNotInRange(Int32 minuteValue)
        {
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => DynamicRenewTime.WithSpecificRange(13, minuteValue, 20, 30));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(9)]
        [InlineData(24 * 60 + 1)]
        public void DynamicRenewTime_Failed_ReboundNotInRange(Int32 reboundValue)
        {
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => DynamicRenewTime.WithSpecificRange(13, 10, reboundValue, 30));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(14)]
        [InlineData(24 * 60 + 1)]
        public void DynamicRenewTime_Failed_EndOfLifeNotInRange(Int32 lifetime)
        {
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => DynamicRenewTime.WithSpecificRange(13, 10, 45, lifetime));
        }

        [Theory]
        [InlineData(20,15)]
        [InlineData(40,30)]
        public void DynamicRenewTime_Failed_ReboundGreaterThanLifeTime(Int32 reboundValue, Int32 lifetime)
        {
            Assert.ThrowsAny<ArgumentException>(() => DynamicRenewTime.WithSpecificRange(13, 10, reboundValue, lifetime));
        }

        [Fact]
        public void DynamicRenewTime_GetLeaseTimers()
        {
            DateTime now = DateTime.Now;

            DateTime expectedLeaseTime = now.AddHours(2).AddMinutes(30);
            // add ten seconds as for the "runtime" of the test
            TimeSpan smallestExpectTimeRange = (expectedLeaseTime - now).Add(TimeSpan.FromMinutes(-10)).Add(TimeSpan.FromSeconds(-30));
            TimeSpan greatestExpectTimeRange = (expectedLeaseTime - now).Add(TimeSpan.FromMinutes(10)).Add(TimeSpan.FromSeconds(30));

            var time = DynamicRenewTime.WithSpecificRange(expectedLeaseTime.Hour, expectedLeaseTime.Minute, 30, 60);

            for (int i = 0; i < 100; i++)
            {
                var spans = time.GetLeaseTimers();

                Assert.True(spans.RenewTime >= smallestExpectTimeRange);
                Assert.True(spans.RenewTime <= greatestExpectTimeRange);

                Assert.Equal(spans.RenewTime + TimeSpan.FromMinutes(30), spans.ReboundTime);
                Assert.Equal(spans.RenewTime + TimeSpan.FromMinutes(60), spans.Lifespan);
            }
        }

        [Fact]
        public void DynamicRenewTime_GetLeaseTimers_WithDayOverflow()
        {
            DateTime now = DateTime.Now;

            DateTime expectedLeaseTime = now.AddHours(-2).AddMinutes(-30);
            TimeSpan middleExpectedLeasetime = (TimeSpan.FromDays(1).Add(TimeSpan.FromHours(-2)).Add(TimeSpan.FromMinutes(-30)));
            // add ten seconds as for the "runtime" of the test
            TimeSpan smallestExpectTimeRange = middleExpectedLeasetime.Add(TimeSpan.FromMinutes(-10)).Add(TimeSpan.FromSeconds(-60));
            TimeSpan greatestExpectTimeRange = middleExpectedLeasetime.Add(TimeSpan.FromMinutes(10)).Add(TimeSpan.FromSeconds(60));

            var time = DynamicRenewTime.WithSpecificRange(expectedLeaseTime.Hour, expectedLeaseTime.Minute, 30, 60);

            for (int i = 0; i < 100; i++)
            {
                var spans = time.GetLeaseTimers();

                Assert.True(spans.RenewTime >= smallestExpectTimeRange);
                Assert.True(spans.RenewTime <= greatestExpectTimeRange);

                Assert.Equal(spans.RenewTime + TimeSpan.FromMinutes(30), spans.ReboundTime);
                Assert.Equal(spans.RenewTime + TimeSpan.FromMinutes(60), spans.Lifespan);
            }
        }

        [Fact]
        public void DynamicRenewTime_GetLeaseTimers_WithShortTime()
        {
            DateTime now = DateTime.Now;

            DateTime expectedLeaseTime = now.AddMinutes(-5);
            TimeSpan middleExpectedLeasetime = (TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(-5)));
            // add ten seconds as for the "runtime" of the test
            TimeSpan smallestExpectTimeRange = middleExpectedLeasetime.Add(TimeSpan.FromMinutes(-10)).Add(TimeSpan.FromSeconds(-60));
            TimeSpan greatestExpectTimeRange = middleExpectedLeasetime.Add(TimeSpan.FromMinutes(10)).Add(TimeSpan.FromSeconds(60));

            var time = DynamicRenewTime.WithSpecificRange(expectedLeaseTime.Hour, expectedLeaseTime.Minute, 30, 60);

            for (int i = 0; i < 100; i++)
            {
                var spans = time.GetLeaseTimers();

                Assert.True(spans.RenewTime >= smallestExpectTimeRange);
                Assert.True(spans.RenewTime <= greatestExpectTimeRange);

                Assert.Equal(spans.RenewTime + TimeSpan.FromMinutes(30), spans.ReboundTime);
                Assert.Equal(spans.RenewTime + TimeSpan.FromMinutes(60), spans.Lifespan);
            }
        }
    }
}
