using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes;
using Beer.DaAPI.BlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public class DHCPv6ExportRequest
    {
        public Guid Id { get; set; }
        public CreateOrUpdateDHCPv6ScopeRequest Request { get; set; }
    }

    public partial class DHCPv6ExportScopeStructureDialog : DaAPIDialogBase
    {
        [Inject] public DaAPIService Service { get; set; }
        private Boolean _loading = true;

        private String _content;

        private List<DHCPv6ExportRequest> _requests = new();
        private IEnumerable<DHCPv6ScopeResolverDescription> _resolverDescriptions;

        private async Task GetDetails(DHCPv6ScopeTreeViewItem item, DHCPv6ScopePropertiesResponse parentValue)
        {
            var details = await Service.GetDHCPv6ScopeProperties(item.Id, false);

            CreateOrUpdateDHCPv6ScopeViewModel vm = new()
            {
                GenerellProperties = new CreateOrUpdateDHCPv6ScopeGenerellPropertiesViewModel(),
                AddressRelatedProperties = new CreateOrUpdateDHCPv6ScopeAddressRelatedPropertiesViewModel(),
                ResolverProperties = new CreateOrUpdateDHCPv6ScopeResolverRelatedViewModel(),
                ScopeProperties = new CreateOrUpdateDHCPv6ScopePropertiesViewModel(),
            };

            vm.GenerellProperties.Name = details.Name;
            vm.GenerellProperties.Description = details.Description;
            vm.GenerellProperties.HasParent = details.ParentId.HasValue;
            vm.GenerellProperties.Id = item.Id;
            vm.GenerellProperties.ParentId = details.ParentId;

            if (parentValue != null)
            {
                vm.AddressRelatedProperties.SetParent(parentValue);
                vm.ScopeProperties.LoadFromParent(parentValue, details);
            }

            vm.AddressRelatedProperties.SetByResponse(details);
            vm.ScopeProperties.LoadFromResponse(details);
            vm.ResolverProperties.LoadFromResponse(details, _resolverDescriptions.FirstOrDefault(x => x.TypeName == details.Resolver.Typename));


            _requests.Add(new DHCPv6ExportRequest
            {
                Id = item.Id,
                Request = vm.GetRequest()
            });

            if (item.ChildScopes.Any() == true)
            {

                if (details.ParentId.HasValue == true)
                {
                    details = await Service.GetDHCPv6ScopeProperties(item.Id, true);
                }

                foreach (var child in item.ChildScopes)
                {
                    await GetDetails(child, details);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var items = await Service.GetDHCPv6ScopesAsTree();
            _resolverDescriptions = await Service.GetDHCPv6ScopeResolverDescription();
            foreach (var item in items)
            {
                await GetDetails(item, null);
            }

            _content = JsonConvert.SerializeObject(_requests, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),

            });

            _loading = false;
        }

    }
}
