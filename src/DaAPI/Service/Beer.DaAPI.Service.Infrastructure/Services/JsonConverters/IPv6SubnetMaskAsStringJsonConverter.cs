using Beer.DaAPI.Core.Common.DHCPv6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Infrastructure.Services.JsonConverters
{
    public class IPv6SubnetMaskAsStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IPv6SubnetMask);

        public override object ReadJson(JsonReader reader, Type objectType,  object existingValue, JsonSerializer serializer)
        {
            if(reader.Value is not String value)
            {
                if (reader.Value is Int64 numberValue)
                {
                    return new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(Convert.ToByte(numberValue)));
                }

                return new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(0));
            }
            else
            {
                return new IPv6SubnetMask(new IPv6SubnetMaskIdentifier(Convert.ToByte(value)));
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((value as IPv6SubnetMask).Identifier.Value.ToString());
        }
    }
}
