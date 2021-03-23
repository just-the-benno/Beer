using Beer.OIDCOptionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorHost.Configuration
{
    public class AppConfiguration
    {
        public OpenIdConnectionConfiguration OpenIdConnection { get; set; }
        public Dictionary<String,String> APIUrls { get; set; }
    }
}
