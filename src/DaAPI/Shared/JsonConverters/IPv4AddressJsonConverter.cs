using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Infrastructure.StorageEngine.Converters
{
    public class IPv4AddressJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IPv4Address);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            String rawValue = (String)reader.Value;

            if(String.IsNullOrEmpty(rawValue) == true)
            {
                return null;
            }

            return IPv4Address.FromString(rawValue);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPv4Address address = (IPv4Address)value;
            writer.WriteValue(address.ToString());
        }
    }
}
