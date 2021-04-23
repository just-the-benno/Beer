using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class CreateOrUpdateDHCPv4ScopeViewModel
    {
        public CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel GenerellProperties { get; set; }
        public CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel AddressRelatedProperties { get; set; }
        public CreateOrUpdateDHCPv4ScopePropertiesViewModel ScopeProperties { get; set; }
        public CreateOrUpdateDHCPv4ScopeResolverRelatedViewModel ResolverProperties { get; set; }

        public CreateOrUpdateDHCPv4ScopeRequest GetRequest()
        {
            var request = new CreateOrUpdateDHCPv4ScopeRequest
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
