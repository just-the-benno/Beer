using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Shared.Responses
{
    public static class LocalUsersResponses
    {
        public static class V1
        {
            public class LocalUserOverview
            {
                public String Id { get; set; }
                public String LoginName { get; set; }
                public String DisplayName { get; set; }
            }
        }
    }
}
