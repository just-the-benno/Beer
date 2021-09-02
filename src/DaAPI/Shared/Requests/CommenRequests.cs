using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Shared.Requests
{
    public static class CommenRequests
    {
        public static class V1
        {
            public class FilterLeaseHistoryRequest
            {
                public Guid? ScopeId { get; set; }
                public Boolean? IncludeChildren { get; set; }
                public String Address { get; set; }
                public DateTime? StartTime { get; set; }
                public DateTime? EndTime { get; set; }
                public Int32 Start { get; set; } = 0;
                public Int32 Amount { get; set; } = 30;
            }



        }
    }
}
