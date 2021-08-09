using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Services
{
   
    public interface IDHCPv6ServerPropertiesResolver
    {
        DUID GetServerDuid();
        TimeSpan GetLeaseLifeTime();
        TimeSpan GetHandledLifeTime();
        UInt32 GetMaximumHandledCounter();
        TimeSpan GetTracingStreamLifeTime();

    }
}
