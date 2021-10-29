using Beer.DaAPI.Core.Notifications.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Services
{
    public interface IDHCPv6PrefixCollector
    {
        Task<IEnumerable<(Guid,PrefixBinding)>> GetActiveDHCPv6Prefixes();
    }
}
