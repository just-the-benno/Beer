using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Services.Responses
{
    public static class BeerUserResponses
    {
        public static class V1
        {
            public class BeerUserOverview
            {
                public String Id { get; set; }
                public String LoginName { get; set; }
                public String DisplayName { get; set; }
            }
        }
    }
}
