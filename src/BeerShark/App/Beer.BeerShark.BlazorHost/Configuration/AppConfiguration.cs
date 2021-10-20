using Beer.OIDCOptionHelper;
using System;
using System.Collections.Generic;

namespace Beer.BeerShark.BlazorHost.Configuration
{
    public class AppConfiguration
    {
        public OpenIdConnectionConfiguration OpenIdConnection { get; set; } = new();
        public Dictionary<String, String> APIUrls { get; set; } = new Dictionary<String, String>();
        public Dictionary<String, String> AppUrls { get; set; } = new Dictionary<String, String>(); 
    }
}
