using Beer.DaAPI.Core.Scopes.DHCPv4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public class DHCPv4ScopePropertyResponseJsonConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new()
        {
            ContractResolver = new BaseSpecifiedConcreteClassConverter<DHCPv4ScopePropertyResponse>()
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv4ScopePropertyResponse);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var token = jo["Type"] ?? jo["type"];
            Int32 rawValue = token.Value<Int32>();

            return (DHCPv4ScopePropertyType)rawValue switch
            {
                DHCPv4ScopePropertyType.AddressList => JsonConvert.DeserializeObject<DHCPv4AddressListScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                DHCPv4ScopePropertyType.Byte or DHCPv4ScopePropertyType.UInt16 or DHCPv4ScopePropertyType.UInt32 => JsonConvert.DeserializeObject<DHCPv4NumericScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                DHCPv4ScopePropertyType.Text => JsonConvert.DeserializeObject<DHCPv4TextScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                DHCPv4ScopePropertyType.Boolean => JsonConvert.DeserializeObject<DHCPv4BooleanScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                DHCPv4ScopePropertyType.Time => JsonConvert.DeserializeObject<DHCPv4TimeScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                DHCPv4ScopePropertyType.Address => JsonConvert.DeserializeObject<DHCPv4AddressScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                _ => throw new NotImplementedException(),
            };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}

