using Beer.ControlCenter.BlazorApp.Services.Requests;
using Beer.ControlCenter.BlazorApp.Services.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Beer.ControlCenter.BlazorApp.Services.Requests.BeerClientRequests.V1;
using static Beer.ControlCenter.BlazorApp.Services.Requests.BeerUserRequests.V1;
using static Beer.ControlCenter.BlazorApp.Services.Responses.BeerClientResponses.V1;
using static Beer.ControlCenter.BlazorApp.Services.Responses.BeerUserResponses.V1;

namespace Beer.ControlCenter.BlazorApp.Services
{
    public class HttpClientBasedOpenIdService : HttpClientBase, IOpenIdService
    {
        private class OpenIdDiscoveryDocumentConverter : JsonConverter<OpenIdEndpoints>
        {
            public override OpenIdEndpoints Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                OpenIdEndpoints result = new();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return result;
                    }

                    // Get the key.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        continue;
                    }

                    string propertyName = reader.GetString();
      

                    reader.Read();
                    if(reader.TokenType != JsonTokenType.String)
                    {
                        continue;
                    }

                    String value = reader.GetString();

                    switch (propertyName)
                    {
                        case "authorization_endpoint":
                            result.Authorization = value;
                            break;
                        case "check_session_iframe":
                            result.CheckSessionIframe = value;
                            break;
                        case "device_authorization_endpoint":
                            result.DeviceAuthorization = value;
                            break;
                        case "end_session_endpoint":
                            result.EndSession = value;
                            break;
                        case "introspection_endpoint":
                            result.Introspection = value;
                            break;
                        case "issuer":
                            result.Issuer = value;
                            break;
                        case "jwks_uri":
                            result.JWKsUri = value;
                            break;
                        case "revocation_endpoint":
                            result.Revocation = value;
                            break;
                        case "token_endpoint":
                            result.Token = value;
                            break;
                        case "userinfo_endpoint":
                            result.Userinfo = value;
                            break;
                        default:
                            break;
                    }
                }

                return result;
            }

            public override void Write(Utf8JsonWriter writer, OpenIdEndpoints value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        public HttpClientBasedOpenIdService(HttpClient client) : base(client)
        {
        }

        public async Task<Boolean> CreateUser(CreateBeerUserRequest request) => await PostAsJsonAsync("/api/LocalUsers/", request);

        public async Task<IEnumerable<ClientOverview>> GetAllOpenIdClients() => await Client.GetFromJsonAsync<IEnumerable<ClientOverview>>("/api/Clients/");

        public async Task<Boolean> CreateClient(CreateBeerOpenIdClientRequest request) => await PostAsJsonAsync("/api/Clients/", request);

        public async Task<Boolean> UpdateClient(UpdateBeerOpenIdClientRequest request) => await PutAsJsonAsync($"/api/Clients/{request.SystemId}", request);

        public async Task<Boolean> DeleteClient(Guid systemId)
        {
            var response = await Client.DeleteAsync($"/api/Clients/{systemId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<OpenIdEndpoints> GetOpenIdConfiguration()
        {
            var response = await Client.GetAsync(".well-known/openid-configuration");

            if(response.IsSuccessStatusCode == false)
            {
                return null;
            }

            JsonSerializerOptions options = new();
            options.Converters.Add(new OpenIdDiscoveryDocumentConverter());

            OpenIdEndpoints result =  await response.Content.ReadFromJsonAsync<OpenIdEndpoints>(options);
            return result;
        }
    }
}
