using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Shared.Components.Nav
{
    public class NavItemGroupViewModel
    {
        public String Caption { get; set; }
        public Boolean DisplayCaption { get; set; }
        public IEnumerable<NavItemViewModel> NavItems { get; set; }
    }
}
