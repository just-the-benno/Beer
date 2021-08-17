using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp
{
    public class EndpointOptions
    {
        public Uri ApiEndpoint { get; set; }
        public Uri HubEndpoint { get; set; }
    }
}
