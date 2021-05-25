using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.Converters;
using Beer.TestHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Beer.DaAPI.UnitTests.Infrastructure.StorageEngine.Converters
{
    public class DHCPv4ScopeAddressPropertiesConverterTester
    {
        [Fact]
        public void SerializeAndDeserialize_WithoutDynamicRenew()
        {
            Random random = new Random();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv4AddressJsonConverter());
            settings.Converters.Add(new DHCPv4ScopeAddressPropertiesConverter());

            var input = new DHCPv4ScopeAddressProperties(
                       IPv4Address.FromString("192.168.10.20"), IPv4Address.FromString("192.168.10.50"),
                       new List<IPv4Address> { IPv4Address.FromString("192.168.10.30"), IPv4Address.FromString("192.168.10.34") },
                        TimeSpan.FromMinutes(random.Next(10, 20)), TimeSpan.FromMinutes(random.Next(40, 60)), TimeSpan.FromMinutes(random.Next(80, 120)),
                        (Byte)random.Next(10, 30), random.NextBoolean(), DaAPI.Core.Scopes.ScopeAddressProperties<DHCPv4ScopeAddressProperties, IPv4Address>.AddressAllocationStrategies.Next, random.NextBoolean(), random.NextBoolean(), random.NextBoolean());
            
            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv4ScopeAddressProperties>(serialized, settings);

            Assert.Equal(input, actual);
        }

        [Fact]
        public void SerializeAndDeserialize_WithDynamicRenew()
        {
            Random random = new Random();

            DynamicRenewTime time = DynamicRenewTime.WithSpecificRange(random.Next(1, 20), random.Next(10, 60), random.Next(20, 40), random.Next(50, 60));

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPv4AddressJsonConverter());
            settings.Converters.Add(new DHCPv4ScopeAddressPropertiesConverter());

            var input = new DHCPv4ScopeAddressProperties(
                       IPv4Address.FromString("192.168.10.20"), IPv4Address.FromString("192.168.10.50"),
                       new List<IPv4Address> { IPv4Address.FromString("192.168.10.30"), IPv4Address.FromString("192.168.10.34") },
                        time,
                        (Byte)random.Next(10, 30), random.NextBoolean(), DaAPI.Core.Scopes.ScopeAddressProperties<DHCPv4ScopeAddressProperties, IPv4Address>.AddressAllocationStrategies.Next, random.NextBoolean(), random.NextBoolean(), random.NextBoolean());

            String serialized = JsonConvert.SerializeObject(input, settings);
            var actual = JsonConvert.DeserializeObject<DHCPv4ScopeAddressProperties>(serialized, settings);

            Assert.Equal(input, actual);
        }
    }
}
