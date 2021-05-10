using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Shared.Requests
{
    public static class ClientRequests
    {
        public static class V1
        {
            public abstract class CreateOrUpdateClientRequest
            {
                [Required]
                [StringLength(50, MinimumLength = 3)]
                public String DisplayName { get; set; }

                [Required]
                [StringLength(50, MinimumLength = 3)]
                public String ClientId { get; set; }

                [Required]
                [StringLength(50, MinimumLength = 3)]
                public String Password { get; set; }

                public Boolean RequirePkce { get; set; }

                [Required]
                [MinLength(1)]
                [MaxLength(30)]
                public IEnumerable<String> RedirectUris { get; set; }

                [MaxLength(30)]
                public IEnumerable<String> AllowedCorsOrigins { get; set; }

                public String FrontChannelLogoutUri { get; set; }

                [Required]
                [MinLength(1)]
                [MaxLength(30)]
                public IEnumerable<String> PostLogoutRedirectUris { get; set; }

                [MaxLength(30)]
                public IEnumerable<String> AllowedScopes { get; set; }
            }

            public class CreateClientRequest : CreateOrUpdateClientRequest
            {
               
            }

            public class UpdateClientRequest : CreateOrUpdateClientRequest
            {
                public Guid SystemId { get; set; }
            }
        }
    }
}
