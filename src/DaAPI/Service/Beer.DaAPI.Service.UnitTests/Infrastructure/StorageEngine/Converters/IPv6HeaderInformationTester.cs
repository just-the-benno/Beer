using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine.Converters;
using Beer.DaAPI.Service.TestHelper;
using Beer.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Beer.DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class IPv6HeaderInformationTester
    {
        [Fact]
        public void SerializeAndDeserialize()
        {
            Random random = new Random();
            var input = new IPv6HeaderInformation(
                random.GetIPv6Address(),random.GetIPv6Address());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<IPv6HeaderInformation>(serialized, settings);

            Assert.Equal(input.Source, actual.Source);
            Assert.Equal(input.Destionation, actual.Destionation);
        }

        [Fact]
        public void SerializeAndDeserialize_Null()
        {
            IPv6HeaderInformation input = null;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<IPv6HeaderInformation>(serialized, settings);

            Assert.Null(actual);
        }
    }
}
