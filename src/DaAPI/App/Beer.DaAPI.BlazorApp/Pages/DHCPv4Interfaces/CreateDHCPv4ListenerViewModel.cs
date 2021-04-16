using Beer.DaAPI.BlazorApp.ModelHelper;
using System;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Interfaces
{
    public class CreateDHCPv4ListenerViewModel
    {
        public String Name { get; set; }
        public String IPv4Address { get; set; }
        public String InterfaceId { get; set; }
    }
}
