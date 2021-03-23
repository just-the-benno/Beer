using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine.Converters;
using Beer.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Beer.DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6PrefixDelgationInfoJsonConverterTester
    {
        [Fact]
        public void SerializeAndDeserialize()
        {
            var input = DHCPv6PrefixDelgationInfo.FromValues(IPv6Address.FromString("2001:e68:5423:5ffd::0"), new IPv6SubnetMaskIdentifier(64), new IPv6SubnetMaskIdentifier(70));

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new DHCPv6PrefixDelgationInfoJsonConverter());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv6PrefixDelgationInfo>(serialized, settings);

            Assert.Equal(input, actual);
        }

        [Fact]
        public void SerializeAndDeserialize_Null()
        {
            DHCPv6PrefixDelgationInfo input = null;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new DHCPv6PrefixDelgationInfoJsonConverter());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv6PrefixDelgationInfo>(serialized, settings);

            Assert.Equal(input, actual);
        }

    }
}
