﻿using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Core.Tracing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
            if (_connected == true) { return Task.FromResult(true); }

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

        private async Task<(Boolean, String)> ExecuteCLICommand(String command, Boolean returnRawValue, TracingStream tracingStream)
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
                return (false, String.Empty);
            }

            String rawContent = await result.Content.ReadAsStringAsync();
            NxOsDeviceResponse response = null;
            if (returnRawValue == false || result.IsSuccessStatusCode == false)
            {
                response = JsonConvert.DeserializeObject<NxOsDeviceResponse>(rawContent);
            }
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
                return (false, rawContent);
            }

            await tracingStream.Append(5, TracingRecordStatus.Success);

            return (true, rawContent);

        }

        public async Task<Boolean> AddIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host, TracingStream tracingStream)
        {
            String command = $"ipv6 route {prefix}/{length} {host} 60";
            _logger.LogDebug("adding a static ipv6 route with {command}", command);

            Boolean result = (await ExecuteCLICommand(command, false, tracingStream)).Item1;
            return result;
        }

        public async Task<Boolean> RemoveIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host, TracingStream tracingStream)
        {
            String command = $"no ipv6 route {prefix}/{length} {host} 60";

            _logger.LogDebug("removing a static ipv6 route with {command}", command);

            Boolean result = (await ExecuteCLICommand(command, false, tracingStream)).Item1;
            return result;
        }

        public int GetTracingIdenfier() => 1;

        public IEnumerable<PrefixBinding> ParseIPv6RouteJson(String input, Boolean asCompletedDocument)
        {
            List<PrefixBinding> existingBindings = new List<PrefixBinding>();

            JsonDocument document = JsonDocument.Parse(input);

            var root = document.RootElement;
            if (asCompletedDocument == true)
            {
                root = root.GetProperty("result");
            }

            var routeElements = root
                .GetProperty("body")
                .GetProperty("TABLE_vrf")
                .GetProperty("ROW_vrf")
                .GetProperty("TABLE_addrf")
                .GetProperty("ROW_addrf")
                .GetProperty("TABLE_prefix")
                .GetProperty("ROW_prefix");

            Int32 itemAmount = routeElements.GetArrayLength();

            var array = routeElements.EnumerateArray();

            foreach (var item in array)
            {
                var prefix = item.GetProperty("ipprefix").GetString();
                if (prefix.Contains('/') == false) { continue; }
                var parts = prefix.Split('/');

                var prefixPart = parts[0];
                byte length = Convert.ToByte(parts[1]);

                var rowPathProperty = item.GetProperty("TABLE_path").GetProperty("ROW_path");

                if (rowPathProperty.ValueKind != JsonValueKind.Array)
                {
                    if (rowPathProperty.TryGetProperty("ipnexthop", out JsonElement ipnextHopProperty) == false)
                    {
                        continue;
                    }

                    var hostAddress = ipnextHopProperty.GetString();

                    existingBindings.Add(new PrefixBinding(
                         IPv6Address.FromString(prefixPart), new IPv6SubnetMaskIdentifier(length), IPv6Address.FromString(hostAddress)));
                }
                else
                {
                    foreach (var hostEntry in rowPathProperty.EnumerateArray())
                    {
                        if (hostEntry.TryGetProperty("ipnexthop", out JsonElement ipnextHopProperty) == false)
                        {
                            continue;
                        }

                        var hostAddress = ipnextHopProperty.GetString();

                        existingBindings.Add(new PrefixBinding(
                            IPv6Address.FromString(prefixPart), new IPv6SubnetMaskIdentifier(length), IPv6Address.FromString(hostAddress)));
                    }
                }
            }

            return existingBindings;
        }

        public async Task<IEnumerable<PrefixBinding>> CleanupRoutingTable(IEnumerable<PrefixBinding> bindings, TracingStream tracingStream)
        {
            var cliPreResult = await ExecuteCLICommand("show ipv6 route static", true, tracingStream);

            if (cliPreResult.Item1 == false)
            {
                return Array.Empty<PrefixBinding>();
            }

            var existingPrefixes = ParseIPv6RouteJson(cliPreResult.Item2, true);
            List<PrefixBinding> bindingsToAdd = new List<PrefixBinding>(bindings);
            await tracingStream.Append(11, TracingRecordStatus.Informative, new Dictionary<String,String>
            {
                { "ExistingRouteCount", existingPrefixes.Count().ToString() }
            });

            Int32 removeCounter = 0;

            foreach (var item in existingPrefixes)
            {
                var binding = bindingsToAdd.FirstOrDefault(x => x.Prefix == item.Prefix && x.Host == item.Host && x.Mask == item.Mask);
                if(binding == null)
                {
                    await RemoveIPv6StaticRoute(item.Prefix, item.Mask.Identifier, item.Host, tracingStream);
                    removeCounter++;
                }
                else
                {
                    bindingsToAdd.Remove(binding);
                }
            }

            await tracingStream.Append(12, TracingRecordStatus.Informative, new Dictionary<String, String>
            {
                { "ExistingRouteCount", existingPrefixes.Count().ToString() },
                { "RemoveRouteCounter", removeCounter.ToString() },
                { "RoutesToAddCounter", bindingsToAdd.Count.ToString() },
            });

            return bindingsToAdd;
        }
    }
}
