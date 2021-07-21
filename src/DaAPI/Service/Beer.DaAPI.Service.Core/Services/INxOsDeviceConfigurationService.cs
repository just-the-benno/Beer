using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Services
{
    public interface INxOsDeviceConfigurationService
    {
        Task<Boolean> Connect(String iPAddress, String username, String password, TracingStream tracingStream);
        Task<Boolean> RemoveIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host, TracingStream tracingStream);
        Task<Boolean> AddIPv6StaticRoute(IPv6Address prefix, IPv6SubnetMaskIdentifier length, IPv6Address host, TracingStream tracingStream);
        Int32 GetTracingIdenfier();
    }
}
