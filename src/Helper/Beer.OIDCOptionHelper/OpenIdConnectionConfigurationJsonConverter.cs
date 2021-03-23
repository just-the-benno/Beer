using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beer.OIDCOptionHelper
{
    public class OpenIdConnectionConfigurationJsonConverter : JsonConverter<OpenIdConnectionConfiguration>
    {
        public override OpenIdConnectionConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, OpenIdConnectionConfiguration value, JsonSerializerOptions options)
        {
            var uniqueScopes = new HashSet<String>(value.DefaultScopes.Union(value.Scopes));

            String scopes = String.Empty;
            foreach (var item in uniqueScopes)
            {
                scopes += $"{item} ";
            }

            var realObject = new
            {
                authority = value.Authority,
                metadataUrl = value.MetadataUrl,
                client_id = value.ClientId,
                redirect_uri = value.RedirectUri,
                post_logout_redirect_uri = value.PostLogoutRedirectUri,
                response_type = value.ResponseType,
                response_mode = value.ResponseMode,
                scope = scopes.Substring(0, scopes.Length - 1),
            };

            writer.WriteStartObject();
            foreach (var item in realObject.GetType().GetProperties())
            {
                String propertyName = item.Name;
                String propertyValue = item.GetValue(realObject) as String;
                if (String.IsNullOrEmpty(propertyValue) == true)
                {
                    continue;
                }

                writer.WriteString(propertyName, propertyValue);
            }

            writer.WriteEndObject();
        }
    }
}
