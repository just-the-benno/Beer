using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Shared.Responses
{
    public static class DeviceResponses
    {
        public static class V1
        {
            public class DeviceOverviewResponse
            {
                public Guid Id { get; set; }
                public String Name { get; set; }
            }
        }
    }
}
