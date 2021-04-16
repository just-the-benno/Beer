using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.BlazorApp.Resources;
using Beer.DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Interfaces
{
    public class CreateDHCPv6ListenerViewModel
    {
        public String Name { get; set; }
        public String IPv6Address { get; set; }
        public String InterfaceId { get; set; }
    }
}
