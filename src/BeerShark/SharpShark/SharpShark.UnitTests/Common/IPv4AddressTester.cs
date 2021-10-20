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
    public class IPv4AddressTester
    {
        [Fact]
        public void IPv4Address_Equals()
        {
            Random random = new();
            Byte[] firstAddressBytes = random.NextBytes(4);
            Byte[] secondAddressBytes = new Byte[4];
            firstAddressBytes.CopyTo(secondAddressBytes, 0);

            IPv4Address firstAddress = new(firstAddressBytes);
            IPv4Address secondAddress = new(secondAddressBytes);

            Boolean result = firstAddress.Equals(secondAddress);
            result.Should().BeTrue();

            Assert.Equal(firstAddress, secondAddress);

            Byte[] thirdAddressBytes = random.NextBytes(4);
            IPv4Address thirdAddress = new(thirdAddressBytes);

            Boolean nonEqualResult = firstAddress.Equals(thirdAddress);
            nonEqualResult.Should().BeFalse();

            firstAddress.Should().NotBe(thirdAddress);

            (firstAddress == secondAddress).Should().BeTrue();
            (firstAddress != secondAddress).Should().BeFalse();

            (firstAddress == thirdAddress).Should().BeFalse();
            (firstAddress != thirdAddress).Should().BeTrue();
        }

        [Theory]
        [InlineData("194.22.0.0", "194.22.0.10", "195.20.0.20")]
        [InlineData("172.16.16.16", "172.16.16.17", "172.16.16.18")]
        [InlineData("0.0.0.0", "172.16.16.17", "255.255.255.255")]
        public void IPv4Adress_Compare(String small, String middle, String great)
        {
            IPv4Address smallestAddress = new(small);
            IPv4Address middleAddress = new(middle);
            IPv4Address greatestAddress = new(great);

            {
                Int32 firstResult = smallestAddress.CompareTo(smallestAddress);
                Int32 secondResult = smallestAddress.CompareTo(middleAddress);
                Int32 thirdResult = smallestAddress.CompareTo(greatestAddress);

                firstResult.Should().Be(0);
                secondResult.Should().BeNegative();
                thirdResult.Should().BeNegative();

                (smallestAddress < middleAddress).Should().BeTrue();
                (smallestAddress < greatestAddress).Should().BeTrue();

                (smallestAddress > middleAddress).Should().BeFalse();
                (smallestAddress > greatestAddress).Should().BeFalse();

#pragma warning disable CS1718 // Comparison made to same variable
                (smallestAddress <= smallestAddress).Should().BeTrue();
                (smallestAddress >= smallestAddress).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable

            }

            {
                Int32 firstResult = middleAddress.CompareTo(smallestAddress);
                Int32 secondResult = middleAddress.CompareTo(middleAddress);
                Int32 thirdResult = middleAddress.CompareTo(greatestAddress);

                firstResult.Should().BePositive();
                secondResult.Should().Be(0);
                thirdResult.Should().BeNegative();

                (middleAddress > smallestAddress).Should().BeTrue();
                (middleAddress < greatestAddress).Should().BeTrue();

                (middleAddress > greatestAddress).Should().BeFalse();
                (middleAddress < smallestAddress).Should().BeFalse();

#pragma warning disable CS1718 // Comparison made to same variable
                (middleAddress <= middleAddress).Should().BeTrue();
                (middleAddress >= middleAddress).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable

            }

            {
                Int32 firstResult = greatestAddress.CompareTo(smallestAddress);
                Int32 secondResult = greatestAddress.CompareTo(middleAddress);
                Int32 thirdResult = greatestAddress.CompareTo(greatestAddress);

                firstResult.Should().BePositive();
                secondResult.Should().BePositive();
                thirdResult.Should().Be(0);

                (greatestAddress > smallestAddress).Should().BeTrue();
                (greatestAddress > middleAddress).Should().BeTrue();

                (greatestAddress < smallestAddress).Should().BeFalse(); ;
                (greatestAddress < middleAddress).Should().BeFalse(); ;

#pragma warning disable CS1718 // Comparison made to same variable
                (greatestAddress <= greatestAddress).Should().BeTrue();
                (greatestAddress >= greatestAddress).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable

            }
        }

        [Theory]
        [InlineData("100.100.100.100", true, new Byte[] { 100, 100, 100, 100 })]
        [InlineData("10.11.44.255", true, new Byte[] { 10, 11, 44, 255 })]
        [InlineData("0.0.0.0", true, new Byte[] { 0, 0, 0, 0 })]
        [InlineData("255.255.255.255", true, new Byte[] { 255, 255, 255, 255 })]
        [InlineData("255.255.255", false, new Byte[0])]
        [InlineData("255.253.255.14.40", false, new Byte[0])]
        [InlineData("255.256.255.14", false, new Byte[0])]
        [InlineData("255.256.255.-14", false, new Byte[0])]
        [InlineData("255.256.255", false, new Byte[0])]
        [InlineData("", false, new Byte[0])]
        [InlineData(null, false, new Byte[0])]
        [InlineData("     ", false, new Byte[0])]
        public void IPv4Address_FromString(String input, Boolean parseable, Byte[] expectedByteSequence)
        {
            if (parseable == false)
            {
                Assert.ThrowsAny<Exception>(() => new IPv4Address(input));
                return;
            }

            IPv4Address address = new(input);
            Byte[] actual = address.GetBytes();
            Assert.Equal(expectedByteSequence, actual);
        }
    }
}
