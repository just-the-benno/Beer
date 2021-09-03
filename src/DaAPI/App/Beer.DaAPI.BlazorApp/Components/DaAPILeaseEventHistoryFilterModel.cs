using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Components
{
    public class DaAPILeaseEventHistoryFilterModel
    {
        public DateTime? From { get; set; }
        public TimeSpan? FromTime { get; set; }
        public DateTime? To { get; set; }
        public TimeSpan? ToTime { get; set; }
        public Boolean? IncludeChildScopes { get; set; } = true;
        public String Address { get; set; }
    }
}
