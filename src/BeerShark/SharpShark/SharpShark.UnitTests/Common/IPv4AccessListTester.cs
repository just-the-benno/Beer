using FluentAssertions;
using SharpShark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SharpShark.UnitTests.Common
{
    public class IPv4AccessListTester
    {
        [Fact]
        public void EmptyList_DefaultsToFalse()
        {
            var list = new IPv4AccessList(Array.Empty<IPv4AccessListEntry>());

            IPv4Address address = new IPv4Address("10.10.10.10");

            list.IsMatch(address).Should().BeFalse();

        }

        [Fact]
        public void NoMatchingEntries_DefaultsToFalse()
        {
            var list = new IPv4AccessList(new []
            {
               new IPv4AccessListEntry(new IPv4Address("192.168.170.0"),new IPv4SubnetMask(24)),
               new IPv4AccessListEntry(new IPv4Address("192.168.171.0"),new IPv4SubnetMask(24)),
            });

            IPv4Address address = new IPv4Address("192.168.172.3");

            list.IsMatch(address).Should().BeFalse();

        }

        [Fact]
        public void MatchingEntries_StopAfterFirstMatchingEntry()
        {
            var list = new IPv4AccessList(new[]
            {
               new IPv4AccessListEntry(new IPv4Address("192.168.170.0"),new IPv4SubnetMask(24)),
               new IPv4AccessListEntry(new IPv4Address("192.168.170.0"),new IPv4SubnetMask(29)),
            });

            IPv4Address address = new IPv4Address("192.168.170.1");

            list.IsMatch(address).Should().BeTrue();

        }

        [Fact]
        public void MatchingEntries_Find()
        {
            var list = new IPv4AccessList(new[]
            {
               new IPv4AccessListEntry(new IPv4Address("192.168.170.0"),new IPv4SubnetMask(24)),
               new IPv4AccessListEntry(new IPv4Address("192.168.171.0"),new IPv4SubnetMask(29)),
            });

            IPv4Address address = new IPv4Address("192.168.171.1");

            list.IsMatch(address).Should().BeTrue();

        }

    }
}
