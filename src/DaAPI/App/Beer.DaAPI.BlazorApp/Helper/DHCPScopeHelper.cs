using Beer.DaAPI.BlazorApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public class DHCPScopeHelper
    {
        private void GenereteScopeTreeItems(ICollection<(Int32 Depth, DHCPv4ScopeTreeViewItem Scope)> items, Int32 currentDepth, DHCPv4ScopeTreeViewItem treeItem)
        {
            items.Add((currentDepth, treeItem));
            if (treeItem.ChildScopes.Any() == true)
            {
                foreach (var item in treeItem.ChildScopes)
                {
                    GenereteScopeTreeItems(items, currentDepth + 1, item);
                }
            }
        }

        public async Task<IEnumerable<(Int32 Depth, DHCPv4ScopeTreeViewItem Scope)>> GetDHCPv4scopeAsListWithDepth(DaAPIService service)
        {
            var scopes = await service.GetDHCPv4ScopesAsTree();
            var result =  new List<(Int32 Depth, DHCPv4ScopeTreeViewItem Scope)>();
            foreach (var item in scopes)
            {
                GenereteScopeTreeItems(result, 0, item);
            }

            return result;
        }

        private void GenereteScopeTreeItems(ICollection<(Int32 Depth, DHCPv6ScopeTreeViewItem Scope)> items, Int32 currentDepth, DHCPv6ScopeTreeViewItem treeItem)
        {
            items.Add((currentDepth, treeItem));
            if (treeItem.ChildScopes.Any() == true)
            {
                foreach (var item in treeItem.ChildScopes)
                {
                    GenereteScopeTreeItems(items, currentDepth + 1, item);
                }
            }
        }


        public async Task<IEnumerable<(Int32 Depth, DHCPv6ScopeTreeViewItem Scope)>> GetDHCPv6scopeAsListWithDepth(DaAPIService service)
        {
            var scopes = await service.GetDHCPv6ScopesAsTree();
            var result = new List<(Int32 Depth, DHCPv6ScopeTreeViewItem Scope)>();
            foreach (var item in scopes)
            {
                GenereteScopeTreeItems(result, 0, item);
            }

            return result;
        }
    }
}
