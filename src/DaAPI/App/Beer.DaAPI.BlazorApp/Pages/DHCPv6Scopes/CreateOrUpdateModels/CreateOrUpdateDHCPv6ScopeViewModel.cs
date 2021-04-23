using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public class CreateOrUpdateDHCPv6ScopeViewModel
    {
        public CreateOrUpdateDHCPv6ScopeGenerellPropertiesViewModel GenerellProperties { get; set; }
        public CreateOrUpdateDHCPv6ScopeAddressRelatedPropertiesViewModel AddressRelatedProperties { get; set; }
        public CreateOrUpdateDHCPv6ScopePropertiesViewModel ScopeProperties { get; set; }
        public CreateOrUpdateDHCPv6ScopeResolverRelatedViewModel ResolverProperties { get; set; }

        public CreateOrUpdateDHCPv6ScopeRequest GetRequest()
        {
            var request = new CreateOrUpdateDHCPv6ScopeRequest
            {
                Name = GenerellProperties.Name,
                Description = GenerellProperties.Description,
                ParentId = GenerellProperties.HasParent == false ? new Guid?() : GenerellProperties.ParentId,
                AddressProperties = AddressRelatedProperties.GetRequest(),
                Properties = ScopeProperties.GetRequest(),
                Resolver = ResolverProperties.GetRequest(),
            };

            return request;
        }
    }
}
