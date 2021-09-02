using Beer.DaAPI.Infrastructure.StorageEngine.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Shared.JsonConverters
{
    public static class JsonSerializerSettingsExtentions
    {
        public static void LoadCustomerConverters(this JsonSerializerSettings settings)
        {
            settings.Converters.Add(new DUIDJsonConverter());

            settings.Converters.Add(new DHCPv6PacketJsonConverter());
            settings.Converters.Add(new DHCPv6PrefixDelgationInfoJsonConverter());

            settings.Converters.Add(new DHCPv6ScopeAddressPropertiesConverter());
            settings.Converters.Add(new DHCPv6ScopePropertyJsonConverter());
            settings.Converters.Add(new DHCPv6ScopePropertiesJsonConverter());
            settings.Converters.Add(new DHCPv6ScopePropertyRequestJsonConverter());
            
            settings.Converters.Add(new IPv6AddressJsonConverter());
            settings.Converters.Add(new IPv6HeaderInformationJsonConverter());

            settings.Converters.Add(new DHCPv4PacketJsonConverter());
            settings.Converters.Add(new DHCPv4ScopeAddressPropertiesConverter());
            settings.Converters.Add(new DHCPv4ScopePropertiesJsonConverter());
            settings.Converters.Add(new DHCPv4ScopePropertyRequestJsonConverter());
            settings.Converters.Add(new DHCPv4ScopePropertyJsonConverter());

            settings.Converters.Add(new IPv4AddressJsonConverter());
            settings.Converters.Add(new IPv4HeaderInformationJsonConverter());
            
            settings.Converters.Add(new DUIDJsonConverter());
        }
    }
}
