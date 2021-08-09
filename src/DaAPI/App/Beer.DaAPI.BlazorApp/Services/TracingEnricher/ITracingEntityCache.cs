using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher
{
    public interface ITracingEntityCache
    {
        String GetPipelineName(String pipelineId);
        String GetPipelineLink(String pipelineId);
        String GetDHCPv6ScopeName(String scopeId);
        String GetDHCPv6ScopeLink(String scopeId);
        IEnumerable<String> GetDHPv6ScopeNames(IEnumerable<String> scopeIds);
        IEnumerable<String> GetDHCPv6ScopeLinks(IEnumerable<String> scopeIds);

        Task Initilze();
    }
}
