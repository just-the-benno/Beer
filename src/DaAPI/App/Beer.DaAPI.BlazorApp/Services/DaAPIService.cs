using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Shared.Commands;
using Beer.DaAPI.Shared.Helper;
using Beer.DaAPI.Shared.JsonConverters;
using Beer.DaAPI.Shared.Requests;
using Beer.DaAPI.Shared.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4InterfaceRequests.V1;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static Beer.DaAPI.Shared.Requests.DHCPv6InterfaceRequests.V1;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static Beer.DaAPI.Shared.Requests.NotificationPipelineRequests.V1;
using static Beer.DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static Beer.DaAPI.Shared.Requests.TracingRequests.V1;
using static Beer.DaAPI.Shared.Responses.DeviceResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4InterfaceResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4LeasesResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6InterfaceResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.NotificationPipelineResponses.V1;
using static Beer.DaAPI.Shared.Responses.ServerControllerResponses;
using static Beer.DaAPI.Shared.Responses.ServerControllerResponses.V1;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.BlazorApp.Services
{
    public class DaAPIService
    {
        private readonly HttpClient _client;
        private readonly ILogger<DaAPIService> _logger;

        public DaAPIService(HttpClient client, ILogger<DaAPIService> logger)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static StringContent GetStringContentAsJson<T>(T input)
        {
            String serialziedObject = JsonConvert.SerializeObject(input, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            return new StringContent(
                serialziedObject, Encoding.UTF8, "application/json");
        }

        private static JsonSerializerSettings GetDefaultSettings()
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            settings.Converters.Add(new DUIDJsonConverter());

            return settings;
        }

        public async Task<ServerInitilizedResponse> ServerIsInitilized() => await GetResponse<ServerInitilizedResponse>("api/Server/IsInitialized");
        public async Task<ServerInitilizedResponse> ServerIsInitilized2() => await GetResponse<ServerInitilizedResponse>("api/Server/IsInitialized2");

        public async Task<Boolean> InitilizeServer(InitilizeServeRequest request) =>
            await ExecuteCommand(() => _client.PostAsJsonAsync("api/Server/Initialize", request));

        private async Task<Boolean> ExecuteCommand(Func<Task<HttpResponseMessage>> serviceCaller)
        {
            try
            {
                var result = await serviceCaller();
                return result.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return false;
            }
        }

        public async Task<Boolean> CreateDHCPv6Interface(CreateDHCPv6Listener request) =>
            await ExecuteCommand(() => _client.PostAsJsonAsync("api/interfaces/dhcpv6/", request));

        public async Task<Boolean> SendDeleteDHCPv6InterfaceRequest(Guid interfaceId) =>
            await ExecuteCommand(() => _client.DeleteAsync($"api/interfaces/dhcpv6/{interfaceId}"));

        public async Task<Boolean> CreateDHCPv6Scope(CreateOrUpdateDHCPv6ScopeRequest request) =>
            await ExecuteCommand(() => _client.PostAsync("api/scopes/dhcpv6/", GetStringContentAsJson(request)));

        public async Task<Boolean> SendDeleteNotificationPipelineRequest(Guid pipelineId) =>
            await ExecuteCommand(() => _client.DeleteAsync($"/api/notifications/pipelines/{pipelineId}"));

        public async Task<Boolean> CreateNotificationPipeline(CreateNotifcationPipelineRequest request) =>
            await ExecuteCommand(() => _client.PostAsJsonAsync("/api/notifications/pipelines/", request));

        public async Task<Boolean> SendDeleteDHCPv6ScopeRequest(DHCPv6ScopeDeleteRequest request) =>
            await ExecuteCommand(() => _client.DeleteAsync($"/api/scopes/dhcpv6/{request.Id}/?includeChildren={request.IncludeChildren}"));

        public async Task<Boolean> UpdateDHCPv6Scope(CreateOrUpdateDHCPv6ScopeRequest request, String scopeId) =>
            await ExecuteCommand(() => _client.PutAsync($"api/scopes/dhcpv6/{scopeId}", GetStringContentAsJson(request)));

        public async Task<Boolean> SendChangeDHCPv6ScopeParentRequest(Guid scopeId, Guid? parentId) =>
            await ExecuteCommand(() => _client.PutAsync($"/api/scopes/dhcpv6/changeScopeParent/{scopeId}/{parentId}", GetStringContentAsJson(new { })));

        public async Task<Boolean> SendCancelDHCPv6LeaseRequest(Guid leaseId) =>
          await ExecuteCommand(() => _client.DeleteAsync($"/api/leases/dhcpv6/{leaseId}"));

        private async Task<TResult> GetResponse<TResult>(String url) where TResult : class
        {
            try
            {
                var response = await _client.GetFromJsonAsync<TResult>(url);
                return response;
            }
            catch (Exception)
            {
                _logger.LogError("unable to send service request");
                return null;
            }
        }

        public async Task<DHCPv6InterfaceOverview> GetDHCPv6Interfaces() => await GetResponse<DHCPv6InterfaceOverview>("api/interfaces/dhcpv6");
        public async Task<IEnumerable<DHCPv6ScopeItem>> GetDHCPv6ScopesAsList() => await GetResponse<IEnumerable<DHCPv6ScopeItem>>("api/scopes/dhcpv6/list");
        public async Task<IEnumerable<DHCPv6ScopeTreeViewItem>> GetDHCPv6ScopesAsTree() => await GetResponse<IEnumerable<DHCPv6ScopeTreeViewItem>>("api/scopes/dhcpv6/tree");
        public async Task<IEnumerable<DHCPv6ScopeResolverDescription>> GetDHCPv6ScopeResolverDescription() => await GetResponse<IEnumerable<DHCPv6ScopeResolverDescription>>("api/scopes/dhcpv6/resolvers/description");
        public async Task<DHCPv6ScopePropertiesResponse> GetDHCPv6ScopeProperties(Guid scopeId, Boolean includeParents = true) => await GetDHCPv6ScopeProperties(scopeId.ToString(), includeParents);

        public async Task<DHCPv6ScopePropertiesResponse> GetDHCPv6ScopeProperties(String scopeId, Boolean includeParents = true)
        {
            var response = await _client.GetAsync(
                $"api/scopes/dhcpv6/{scopeId}/properties?includeParents={includeParents}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            String content = await response.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            settings.Converters.Add(new DHCPv6ScopePropertyResponseJsonConverter());

            var result = JsonConvert.DeserializeObject<DHCPv6ScopePropertiesResponse>(
                content, settings);

            return result;
        }


        public async Task<IEnumerable<NotificationPipelineReadModel>> GetNotifactionPipelines() => await GetResponse<IEnumerable<NotificationPipelineReadModel>>("/api/notifications/pipelines/");
        public async Task<NotificationPipelineDescriptions> GetpipelineDescriptions() => await GetResponse<NotificationPipelineDescriptions>("/api/notifications/pipelines/descriptions");

        private async Task<T> GetResult<T>(String url, T fallback)
        {
            try
            {
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode == false)
                {
                    return fallback;
                }

                String rawContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(rawContent, GetDefaultSettings());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return fallback;
            }
        }

        public async Task<IEnumerable<DHCPv6LeaseOverview>> GetDHCPv6LeasesByScope(String scopeId, Boolean includeChildScopes)
        {
            try
            {
                var result = await GetResult($"/api/leases/dhcpv6/scopes/{scopeId}?includeChildren={includeChildScopes}", new List<DHCPv6LeaseOverview>());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return null;
            }
        }

        public async Task<DashboardResponse> GetDashboard() => await GetDashboard<DashboardResponse>();

        public async Task<TDashboard> GetDashboard<TDashboard>() where TDashboard : class
        {
            try
            {
                var result = await GetResult<TDashboard>($"/api/Statistics/Dashboard/", null);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return null;
            }
        }

        private static String AppendTimeRangeToUrl(String url, DateTime? start, DateTime? end)
        {
            if (start.HasValue == true)
            {
                DateTime roundedStart = start.Value.Date;

                url += $"&start={roundedStart:o}";
            }
            if (end.HasValue == true)
            {
                DateTime inclusiveEnd = end.Value.Date.AddDays(+1);
                url += $"&end={inclusiveEnd:o}";
            }

            return url;
        }

        private static String AppendGroupingToUrl(String url, DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            AppendTimeRangeToUrl($"{url}?GroupbBy={group}", start, end);

        public async Task<IDictionary<DateTime, Int32>> GetActiveDHCPv6Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData("/api/Statistics/ActiveDHCPv6Leases", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv6PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData("/api/Statistics/IncomingDHCPv6Packets", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetErrorDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
             await GetSimpleStatisticsData("/api/Statistics/ErrorDHCPv6Packets", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetFileredDHCPv6Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
             await GetSimpleStatisticsData("/api/Statistics/FileredDHCPv6Packets", start, end, group);

        public async Task<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>> GetIncomingDHCPv6PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
                await GetSimpleStatisticsData<IDictionary<DHCPv6PacketTypes, Int32>>("/api/Statistics/IncomingDHCPv6PacketTypes", start, end, group);

        public async Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPV6RequestType(DateTime? start, DateTime? end, DHCPv6PacketTypes packetType) =>
           await GetSimpleStatisticsData<Int32, Int32>(AppendTimeRangeToUrl($"/api/Statistics/ErrorCodesPerDHCPV6RequestType?PacketType={packetType}", start, end));

        private async Task<IDictionary<DateTime, Int32>> GetSimpleStatisticsData(String baseurl, DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData<Int32>(baseurl, start, end, group);

        private async Task<IDictionary<TKey, TValue>> GetSimpleStatisticsData<TKey, TValue>(String url)
        {
            try
            {
                var result = await GetResult(url, new Dictionary<TKey, TValue>());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send service request");
                return null;
            }
        }

        private async Task<IDictionary<DateTime, T>> GetSimpleStatisticsData<T>(String url) =>
            await GetSimpleStatisticsData<DateTime, T>(url);

        private async Task<IDictionary<DateTime, T>> GetSimpleStatisticsData<T>(String baseurl, DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData<T>(AppendGroupingToUrl(baseurl, start, end, group));

        private async Task<IEnumerable<THandeled>> GetHandledDHCPPacketByScopeId<THandeled>(String url) where THandeled : class
        {
            var result = await GetResult<IEnumerable<THandeled>>(url, Array.Empty<THandeled>());
            return result;
        }

        public async Task<IEnumerable<DHCPv6PacketHandledEntry>> GetHandledDHCPv6PacketByScopeId(String scopeId, Int32 amount = 100) => await GetHandledDHCPv6PacketByScopeId<DHCPv6PacketHandledEntry>(scopeId, amount);

        public async Task<IEnumerable<THandeled>> GetHandledDHCPv6PacketByScopeId<THandeled>(String scopeId, Int32 amount = 100) where THandeled : DHCPv6PacketHandledEntry =>
            await GetHandledDHCPPacketByScopeId<THandeled>($"/api/Statistics/HandledDHCPv6Packet/{scopeId}?amount={amount}");


        #region DHCPv4

        public async Task<DHCPv4InterfaceOverview> GetDHCPv4Interfaces() => await GetResponse<DHCPv4InterfaceOverview>("api/interfaces/dhcpv4");

        public async Task<Boolean> CreateDHCPv4Interface(CreateDHCPv4Listener request) =>
                    await ExecuteCommand(() => _client.PostAsJsonAsync("api/interfaces/dhcpv4/", request));

        public async Task<Boolean> SendDeleteDHCPv4InterfaceRequest(Guid interfaceId) =>
            await ExecuteCommand(() => _client.DeleteAsync($"api/interfaces/dhcpv4/{interfaceId}"));

        public async Task<IEnumerable<DHCPv4ScopeItem>> GetDHCPv4ScopesAsList() => await GetResponse<IEnumerable<DHCPv4ScopeItem>>("api/scopes/dhcpv4/list");
        public async Task<IEnumerable<DHCPv4ScopeTreeViewItem>> GetDHCPv4ScopesAsTree() => await GetResponse<IEnumerable<DHCPv4ScopeTreeViewItem>>("api/scopes/dhcpv4/tree");

        public async Task<IEnumerable<DHCPv4ScopeResolverDescription>> GetDHCPv4ScopeResolverDescription() => await GetResponse<IEnumerable<DHCPv4ScopeResolverDescription>>("api/scopes/dhcpv4/resolvers/description");
        public async Task<DHCPv4ScopePropertiesResponse> GetDHCPv4ScopeProperties(Guid scopeId, Boolean includeParents = true) => await GetDHCPv4ScopeProperties(scopeId.ToString(), includeParents);

        public async Task<DHCPv4ScopePropertiesResponse> GetDHCPv4ScopeProperties(String scopeId, Boolean includeParents = true)
        {
            var response = await _client.GetAsync(
                $"api/scopes/dhcpv4/{scopeId}/properties?includeParents={includeParents}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            String content = await response.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            settings.Converters.Add(new DHCPv4ScopePropertyResponseJsonConverter());

            var result = JsonConvert.DeserializeObject<DHCPv4ScopePropertiesResponse>(
                content, settings);

            return result;
        }

        public async Task<Boolean> UpdateDHCPv4Scope(CreateOrUpdateDHCPv4ScopeRequest request, String scopeId) =>
            await ExecuteCommand(() => _client.PutAsync($"api/scopes/dhcpv4/{scopeId}", GetStringContentAsJson(request)));

        public async Task<Boolean> SendDeleteDHCPv4ScopeRequest(DHCPv4ScopeDeleteRequest request) =>
          await ExecuteCommand(() => _client.DeleteAsync($"/api/scopes/dhcpv4/{request.Id}/?includeChildren={request.IncludeChildren}"));


        public async Task<Boolean> CreateDHCPv4Scope(CreateOrUpdateDHCPv4ScopeRequest request) =>
        await ExecuteCommand(() => _client.PostAsync("api/scopes/dhcpv4/", GetStringContentAsJson(request)));

        public async Task<Boolean> SendChangeDHCPv4ScopeParentRequest(Guid scopeId, Guid? parentId) =>
            await ExecuteCommand(() => _client.PutAsync($"/api/scopes/dhcpv4/changeScopeParent/{scopeId}/{parentId}", GetStringContentAsJson(new { })));

        public async Task<Boolean> SendCancelDHCPv4LeaseRequest(Guid leaseId) =>
        await ExecuteCommand(() => _client.DeleteAsync($"/api/leases/dhcpv4/{leaseId}"));

        public async Task<IEnumerable<DHCPv4LeaseOverview>> GetDHCPv4LeasesByScope(String scopeId, Boolean includeChildScopes) =>
        await GetResponse<IEnumerable<DHCPv4LeaseOverview>>($"/api/leases/dhcpv4/scopes/{scopeId}?includeChildren={includeChildScopes}");

        public async Task<IEnumerable<DHCPv4PacketHandledEntry>> GetHandledDHCPv4PacketByScopeId(String scopeId, Int32 amount = 100) => await GetHandledDHCPv4PacketByScopeId<DHCPv4PacketHandledEntry>(scopeId, amount);

        public async Task<IEnumerable<THandeled>> GetHandledDHCPv4PacketByScopeId<THandeled>(String scopeId, Int32 amount = 100) where THandeled : DHCPv4PacketHandledEntry =>
            await GetHandledDHCPPacketByScopeId<THandeled>($"/api/Statistics/HandledDHCPv4Packet/{scopeId}?amount={amount}");

        public async Task<IDictionary<DateTime, Int32>> GetActiveDHCPv4Leases(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
           await GetSimpleStatisticsData("/api/Statistics/ActiveDHCPv4Leases", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetIncomingDHCPv4PacketAmount(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
            await GetSimpleStatisticsData("/api/Statistics/IncomingDHCPv4Packets", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetErrorDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
             await GetSimpleStatisticsData("/api/Statistics/ErrorDHCPv4Packets", start, end, group);

        public async Task<IDictionary<DateTime, Int32>> GetFileredDHCPv4Packets(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
             await GetSimpleStatisticsData("/api/Statistics/FileredDHCPv4Packets", start, end, group);

        public async Task<IDictionary<DateTime, IDictionary<DHCPv6PacketTypes, Int32>>> GetIncomingDHCPv4PacketTypes(DateTime? start, DateTime? end, GroupStatisticsResultBy group) =>
                await GetSimpleStatisticsData<IDictionary<DHCPv6PacketTypes, Int32>>("/api/Statistics/IncomingDHCPv4PacketTypes", start, end, group);

        public async Task<IDictionary<Int32, Int32>> GetErrorCodesPerDHCPv4RequestType(DateTime? start, DateTime? end, DHCPv4MessagesTypes packetType) =>
           await GetSimpleStatisticsData<Int32, Int32>(AppendTimeRangeToUrl($"/api/Statistics/ErrorCodesPerDHCPv4MessageType?MessageType={packetType}", start, end));

        #endregion

        public async Task<IEnumerable<DeviceOverviewResponse>> GetDeviceOverview() => await _client.GetFromJsonAsync<IEnumerable<DeviceOverviewResponse>>("/api/devices/");

        public async Task<FilteredResult<TracingStreamOverview>> GetTracingOverview(FilterTracingRequest request)
        {
            String url = $"/api/tracing/streams?{nameof(FilterTracingRequest.Amount)}={request.Amount}&{nameof(FilterTracingRequest.Start)}={request.Start}";
            if (request.ModuleIdentifier.HasValue == true)
            {
                url += $"&{nameof(FilterTracingRequest.ModuleIdentifier)}={request.ModuleIdentifier.Value}";
            }

            if (request.ProcedureIdentifier.HasValue == true)
            {
                url += $"&{nameof(FilterTracingRequest.ProcedureIdentifier)}={request.ProcedureIdentifier.Value}";
            }

            if (request.StartedBefore.HasValue == true)
            {
                url += $"&{nameof(FilterTracingRequest.StartedBefore)}={request.StartedBefore.Value}";
            }

            if (request.EntitiyId.HasValue == true)
            {
                url += $"&{nameof(FilterTracingRequest.EntitiyId)}={request.EntitiyId.Value}";
            }

            var result = await GetResponse<FilteredResult<TracingStreamOverview>>(url);
            return result;
        }

        public async Task<IEnumerable<TracingStreamRecord>> GetTracingStreamRecords(Guid traceid, Guid? entityId) =>
              await GetResponse<IEnumerable<TracingStreamRecord>>($"/api/tracing/streams/{traceid}/records/{entityId}");

    }
}
