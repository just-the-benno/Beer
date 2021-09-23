using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Beer.DaAPI.Core.Packets.DHCPv4.DHCPv4Packet;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv4.Resolvers
{
    public class DHCPv4MacAddressResolverTester
    {
        [Fact]
        public void DHCPv4MacAddressResolver_GetDescription()
        {
            var resolver = new DHCPv4MacAddressResolver();

            ScopeResolverDescription description = resolver.GetDescription();
            Assert.NotNull(description);

            Assert.Equal(nameof(DHCPv4MacAddressResolver), description.TypeName);

            Assert.NotNull(description.Properties);
            Assert.Equal(2, description.Properties.Count());


            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.First();

                Assert.Equal(nameof(DHCPv4MacAddressResolver.MacAddress), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.ByteArray, propertyDescription.PropertyValueType);
            }
            {
                ScopeResolverPropertyDescription propertyDescription = description.Properties.ElementAt(1);

                Assert.Equal(nameof(DHCPv4MacAddressResolver.SearchClientIdenfifier), propertyDescription.PropertyName);
                Assert.Equal(ScopeResolverPropertyValueTypes.Boolean, propertyDescription.PropertyValueType);
            }
        }

        [Fact]
        public void DHCPv4MacAddressResolver_AreValuesValid_MissingKeys()
        {
            Random random = new Random();

            var mock = new Mock<ISerializer>(MockBehavior.Strict);

            var resolver = new DHCPv4MacAddressResolver();

            List<Dictionary<String, String>> invalidInputs = new List<Dictionary<string, string>>
            {
                null,
                new Dictionary<string, string>(),
                new Dictionary<string, string>()
                {
                    { random.GetAlphanumericString(10), random.GetAlphanumericString(10)   }
                },
                new Dictionary<string, string>()
                {
                    { nameof(DHCPv4MacAddressResolver.MacAddress), "aabbccddeeff" },
                    //{ nameof(DHCPv4MacAddressResolver.SearchClientIdenfifier), "true" },
                },
                 new Dictionary<string, string>()
                {
                    //{ nameof(DHCPv4MacAddressResolver.DeviceId), "ea6a012a-04d9-474c-9ff5-2d9eff61b710" },
                    { nameof(DHCPv4MacAddressResolver.SearchClientIdenfifier), "true" },
                },
            };

            foreach (var item in invalidInputs)
            {
                Boolean result = resolver.ArePropertiesAndValuesValid(item, mock.Object);
                Assert.False(result);
            }
        }

        [Fact]
        public void DHCPv4MacAddressResolver_AreValuesValid_Valid()
        {
            Random random = new Random();

            Boolean searchClientIdenfifier = random.NextBoolean();
            Byte[] macAddress = random.NextBytes(6);

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(ByteHelper.ToString(macAddress, false))).Returns(ByteHelper.ToString(macAddress, false)).Verifiable();

            var resolver = new DHCPv4MacAddressResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4MacAddressResolver.MacAddress),  ByteHelper.ToString(macAddress,false) },
                    { nameof(DHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            Boolean result = resolver.ArePropertiesAndValuesValid(input, serializerMock.Object);
            Assert.True(result);

            serializerMock.Verify();
        }

        [Fact]
        public void DHCPv4MacAddressResolver_ApplyValues()
        {
            Random random = new Random();

            Boolean searchClientIdenfifier = random.NextBoolean();
            Byte[] macAddress = random.NextBytes(6);

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(ByteHelper.ToString(macAddress, false))).Returns(ByteHelper.ToString(macAddress, false)).Verifiable();

            var resolver = new DHCPv4MacAddressResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4MacAddressResolver.MacAddress),  ByteHelper.ToString(macAddress, false) },
                    { nameof(DHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);
            Assert.Equal(searchClientIdenfifier, resolver.SearchClientIdenfifier);
            Assert.Equal(macAddress, resolver.MacAddress);

            serializerMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DHCPv4MacAddressResolver_PacketMeetsConditions_MacAddressInHeader(Boolean shouldPass)
        {
            Random random = new Random();

            Boolean searchClientIdenfifier = random.NextBoolean();

            Byte[] resolverMacAddress = new Byte[] { 0x00, 0xb1, 0xe3, 0xda, 0x24, 0x7f };

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(ByteHelper.ToString(resolverMacAddress, false))).Returns(ByteHelper.ToString(resolverMacAddress, false)).Verifiable();

            var resolver = new DHCPv4MacAddressResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4MacAddressResolver.MacAddress),  ByteHelper.ToString(resolverMacAddress, false) },
                    { nameof(DHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);


            Byte[] resolverMacAddress2 = ByteHelper.CopyData(resolverMacAddress);
            if (shouldPass == false)
            {
                resolverMacAddress2[^1]--;
            }

            DHCPv4Packet packet = new(
                new IPv4HeaderInformation(random.GetIPv4Address(), random.GetIPv4Address()),
                resolverMacAddress2,
                (UInt32)random.Next(),
                IPv4Address.Empty,
                IPv4Address.Empty,
                IPv4Address.Empty,
                DHCPv4PacketFlags.Unicast
                );

            Boolean actual = resolver.PacketMeetsCondition(packet);
            Assert.Equal(shouldPass, actual);

            serializerMock.Verify();
        }



        [Fact]
        public void DHCPv4MacAddressResolver_GetValues()
        {
            Random random = new Random();

            Byte[] macAddress = random.NextBytes(6);
            Boolean searchClientIdenfifier = random.NextBoolean();

            var serializerMock = new Mock<ISerializer>(MockBehavior.Strict);
            serializerMock.Setup(x => x.Deserialze<Boolean>(searchClientIdenfifier.ToString())).Returns(searchClientIdenfifier).Verifiable();
            serializerMock.Setup(x => x.Deserialze<String>(ByteHelper.ToString(macAddress, false))).Returns(ByteHelper.ToString(macAddress, false)).Verifiable();

            var resolver = new DHCPv4MacAddressResolver();

            var input = new Dictionary<string, string>()
                {
                    { nameof(DHCPv4MacAddressResolver.MacAddress), ByteHelper.ToString(macAddress, false) },
                    { nameof(DHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier.ToString() },
                };

            resolver.ApplyValues(input, serializerMock.Object);
            Assert.Equal(searchClientIdenfifier, resolver.SearchClientIdenfifier);
            Assert.Equal(macAddress, resolver.MacAddress);

            var values = resolver.GetValues();
            Assert.Equal(2, values.Count);
            Assert.Equal(new Dictionary<String, String>
            {
                { nameof( DHCPv4MacAddressResolver.MacAddress), ByteHelper.ToString(macAddress, false) },
                { nameof( DHCPv4MacAddressResolver.SearchClientIdenfifier), searchClientIdenfifier == false ? "false" : "true" },

            }, values, new NonStrictDictionaryComparer<String, String>());

            serializerMock.Verify();
        }
    }
}
