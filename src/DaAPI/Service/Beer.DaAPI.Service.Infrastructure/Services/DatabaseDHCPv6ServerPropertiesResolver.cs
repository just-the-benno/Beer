using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.Services
{
    public class DatabaseDHCPv6ServerPropertiesResolver : IDHCPv6ServerPropertiesResolver
    {
        private readonly IDHCPv6ReadStore store;

        public DatabaseDHCPv6ServerPropertiesResolver(IDHCPv6ReadStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        private DHCPv6ServerProperties GetServerConfigModel() => store.GetServerProperties().GetAwaiter().GetResult();

        public DUID GetServerDuid() => GetServerConfigModel().ServerDuid;
        public TimeSpan GetLeaseLifeTime() => GetServerConfigModel().LeaseLifeTime;
        public TimeSpan GetHandledLifeTime() => GetServerConfigModel().HandledLifeTime;
        public UInt32 GetMaximumHandledCounter() => GetServerConfigModel().MaximumHandldedCounter;
        public TimeSpan GetTracingStreamLifeTime() => GetServerConfigModel().TracingStreamLifeTime;
    }
}
