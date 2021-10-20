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
    public class IPv4AccessListEntryTester
    {
        [Theory]
        [InlineData("255.255.255.255", "192.158.55.20", true)]
        [InlineData("255.255.255.255", "192.158.55.24", true)]
        [InlineData("255.255.255.0", "192.158.178.0", true)]
        [InlineData("255.255.255.0", "192.158.178.1", false)]
        [InlineData("255.255.252.0", "172.16.16.0", true)]
        [InlineData("255.255.252.0", "172.16.16.1", false)]
        public void IPv4AccessListEntry_Constructor_SubnetAndAddressMatches(String subnetMaskInput, String addressInput, Boolean expectedResult)
        {
            IPv4SubnetMask mask = new(subnetMaskInput);
            IPv4Address address = new(addressInput);

            if (expectedResult == false)
            {
                Assert.ThrowsAny<Exception>(() => new IPv4AccessListEntry(address, mask));
                return;
            }

            _ = new IPv4AccessListEntry(address, mask);
        }

        [Theory]
        [InlineData("255.255.255.0", "192.158.178.0", "192.158.178.2", true)]
        [InlineData("255.255.255.0", "192.158.178.0", "192.158.177.2", false)]
        [InlineData("255.255.252.0", "172.16.16.0", "172.16.16.10", true)]
        [InlineData("255.255.252.0", "172.16.16.0", "172.16.17.10", true)]
        [InlineData("255.255.252.0", "172.16.16.0", "172.16.15.10", false)]
        public void IPv4AccessListEntry_IsMatch(String subnetMaskInput, String addressInput, String addressToCheckInput, Boolean expectedResult)
        {
            IPv4SubnetMask mask = new(subnetMaskInput);
            IPv4Address address = new(addressInput);

            IPv4Address addressToCheck = new(addressToCheckInput);

            IPv4AccessListEntry entry = new(address, mask);

            var result = entry.IsMatch(addressToCheck);
            result.Should().Be(expectedResult);
        }
    }
}
