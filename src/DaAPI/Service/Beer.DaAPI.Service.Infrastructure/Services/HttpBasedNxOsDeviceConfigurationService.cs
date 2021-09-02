using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Core.Tracing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.Services
{
    public class HttpBasedNxOsDeviceConfigurationService : INxOsDeviceConfigurationService
    {
        private class NxOsDeviceErrorData
        {
            [JsonProperty(PropertyName = "msg")]
            public String Message { get; set; }
        }

        private class NxOsDeviceError
        {
            [JsonProperty(PropertyName = "code")]
            public String Code { get; set; }

            [JsonProperty(PropertyName = "message")]
            public String Message { get; set; }

            [JsonProperty(PropertyName = "data")]
            public NxOsDeviceErrorData Data { get; set; }
        }

        private class NxOsDeviceResponse
        {
            [JsonProperty(PropertyName = "result")]
            public String Result { get; set; }

            [JsonProperty(PropertyName = "error")]
            public NxOsDeviceError Errror { get; set; }
        }

        private HttpClient _client;
        private readonly ILogger<HttpBasedNxOsDeviceConfigurationService> _logger;
        private String _username;
        private Boolean _connected;

        public HttpBasedNxOsDeviceConfigurationService(HttpClient client,
            ILogger<HttpBasedNxOsDeviceConfigurationService> logger)
        {
            this._logger = logger;
            _client = client;
        }

        public Task<Boolean> Connect(String endpoint, String username, String password, TracingStream tracingStream)
        {
            if(_connected == true) { return Task.FromResult(true); }

            String authHeaderValue = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));

            _client.BaseAddress = new Uri(endpoint);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);

            _username = username;
            _connected = true;

            return Task.FromResult(true);
        }

        private StringContent ExecuteCLICommandContent(String cmd)
        {
            String input =
             "[" +
              "{" +
                "\"jsonrpc\": \"2.0\"," +
                "\"method\": \"cli\"," +
                "\"params\": {" +
                            $"\"cmd\": \"{cmd}\"," +
                  "\"version\": 1" +
                "}," +
                "\"id\": 1," +
                "\"rollback\": \"stop-on-error\"" +
              "}" +
            "]";

            var content = new StringContent(input, UTF8Encoding.UTF8, "application/json-rpc");
            return content;
        }

        private async Task<Boolean> ExecuteCLICommand(String command, TracingStream tracingStream)
        {
            await tracingStream.Append(1, TracingRecordStatus.Informative, new Dictionary<String, String>
            {
                { "Command", command },
                { "Url", _client.BaseAddress + "/ins" },
            });

            var result = await _client.PostAsync("/ins", ExecuteCLICommandContent(command));

            await tracingStream.Append(2, TracingRecordStatus.Informative, new Dictionary<String, String>
            {
                { "Command", command },
                { "Url", _client.BaseAddress + "/ins" },
                { "StatusCode", System.Text.Json.JsonSerializer.Serialize(result.StatusCode) },
            });

            _logger.LogDebug("nxos response has code {statusCode}", result.StatusCode);


            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await tracingStream.Append(3, TracingRecordStatus.Error, new Dictionary<String, String>
                {
                    { "Command", command },
                    { "Url", _client.BaseAddress.ToString() },
                    { "Username", _username },
                });

                _logger.LogError("unable to connect to nx os device. user is unauthorized");
                return false;
            }

            String rawContent = await result.Content.ReadAsStringAsync();
            NxOsDeviceResponse response = JsonConvert.DeserializeObject<NxOsDeviceResponse>(rawContent);

            if (result.IsSuccessStatusCode == false)
            {
                await tracingStream.Append(4, TracingRecordStatus.Error, new Dictionary<String, String>
                {
                    { "Command", command },
                    { "Url", _client.BaseAddress.ToString() },
                    { "StatusCode", System.Text.Json.JsonSerializer.Serialize(result.StatusCode) },
                    { "ErrMsg", response?.Errror?.Message },
                    { "ErrMsgExtented", response?.Errror?.Data?.Message },

                });

                _logger.LogDebug("unable to execute command {errMsg}", response?.Errror?.Message + " " + response?.Errror?.Data?.Message);
                return false;
            }

            await tracingStream.Append(5, TracingRecordStatus.Success);

            return response.Errror == null;
        }

        public async Task<Boolean> AddIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host, TracingStream tracingStream)
        {
            String command = $"ipv6 route {prefix}/{length} {host} 60";
            _logger.LogDebug("adding a static ipv6 route with {command}", command);

            Boolean result = await ExecuteCLICommand(command, tracingStream);
            return result;
        }

        public async Task<Boolean> RemoveIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host, TracingStream tracingStream)
        {
            String command = $"no ipv6 route {prefix}/{length} {host} 60";

            _logger.LogDebug("removing a static ipv6 route with {command}", command);

            Boolean result = await ExecuteCLICommand(command, tracingStream);
            return result;
        }

        public int GetTracingIdenfier() => 1;
    }
}
