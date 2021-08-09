using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher
{
    public class SimpleTracingEntityCache : ITracingEntityCache
    {
        private readonly DaAPIService _service;

        public SimpleTracingEntityCache(DaAPIService service)
        {
            this._service = service;
        }

        private Dictionary<String, String> _dhcpv6Scopes = new();
        private Dictionary<string, String> _pipelines = new();

        public string GetDHCPv6ScopeLink(string scopeId) => $"/scopes/dhcpv4/details/{scopeId}";
        public IEnumerable<string> GetDHCPv6ScopeLinks(IEnumerable<string> scopeIds) => scopeIds.Select(x => GetDHCPv6ScopeLink(x)).ToArray();

        private string GetEntityNameOrNull(string entityId, IDictionary<string, String> items)
        {
            if (items.ContainsKey(entityId) == false)
            {
                items.Add(entityId, String.Empty);
            }

            return items[entityId];
        }

        public string GetDHCPv6ScopeName(string scopeId) => GetEntityNameOrNull(scopeId, _dhcpv6Scopes);
        public IEnumerable<string> GetDHPv6ScopeNames(IEnumerable<string> scopeIds) => scopeIds.Select(x => GetDHCPv6ScopeName(x)).ToArray();

        public string GetPipelineLink(string pipelineId) => $"/pipelines/details/{pipelineId}";
        public string GetPipelineName(string pipelineId) => GetEntityNameOrNull(pipelineId, _pipelines);

        public async Task Initilze()
        {
            _pipelines = (await _service.GetNotifactionPipelines()).ToDictionary(x => x.Id.ToString(), x => x.Name);
            _dhcpv6Scopes = (await _service.GetDHCPv6ScopesAsList()).ToDictionary(x => x.Id.ToString(), x => x.Name);
        }
    }
}
