using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public class DHCPv6ScopePropertyResponseJsonConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new()
        {
            ContractResolver = new BaseSpecifiedConcreteClassConverter<DHCPv6ScopePropertyResponse>()
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv6ScopePropertyResponse);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var token = jo["Type"] ?? jo["type"];
            Int32 rawValue = token.Value<Int32>();

            return (DHCPv6ScopePropertyType)rawValue switch
            {
                DHCPv6ScopePropertyType.AddressList => JsonConvert.DeserializeObject<DHCPv6AddressListScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                DHCPv6ScopePropertyType.Byte or DHCPv6ScopePropertyType.UInt16 or DHCPv6ScopePropertyType.UInt32 => JsonConvert.DeserializeObject<DHCPv6NumericScopePropertyResponse>(jo.ToString(), SpecifiedSubclassConversion),
                DHCPv6ScopePropertyType.Text => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}

