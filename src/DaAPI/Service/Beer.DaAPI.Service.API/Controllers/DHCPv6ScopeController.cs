using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv6Scopes;
using Beer.DaAPI.Shared.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.Service.API.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv6ScopeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IScopeResolverManager<DHCPv6Packet, IPv6Address> _resolverManager;
        private readonly DHCPv6RootScope _rootScope;

        public DHCPv6ScopeController(
            IMediator mediator,
            IScopeResolverManager<DHCPv6Packet, IPv6Address> resolverManager,
            DHCPv6RootScope rootScope)
        {
            this._mediator = mediator;
            this._resolverManager = resolverManager;
            _rootScope = rootScope;
        }

        private void GenerateScopeTree(DHCPv6Scope scope, ICollection<DHCPv6ScopeTreeViewItem> parentChildren)
        {
            List<DHCPv6ScopeTreeViewItem> childItems = new List<DHCPv6ScopeTreeViewItem>();
            var node = new DHCPv6ScopeTreeViewItem
            {
                Id = scope.Id,
                StartAddress = scope.AddressRelatedProperties.Start.ToString(),
                EndAddress = scope.AddressRelatedProperties.End.ToString(),
                Name = scope.Name,
                ChildScopes = childItems,
            };

            parentChildren.Add(node);

            if (scope.GetChildScopes().Any())
            {
                foreach (var item in scope.GetChildScopes())
                {
                    GenerateScopeTree(item, childItems);
                }
            }
        }

        [HttpGet("/api/scopes/dhcpv6/tree")]
        public IActionResult GetScopesAsTreeView()
        {
            List<DHCPv6ScopeTreeViewItem> result = new List<DHCPv6ScopeTreeViewItem>();

            foreach (var item in _rootScope.GetRootScopes())
            {
                GenerateScopeTree(item, result);
            }

            return base.Ok(result);
        }

        private void GenerateScopeList(DHCPv6Scope scope, ICollection<DHCPv6ScopeItem> collection)
        {
            var node = new DHCPv6ScopeItem
            {
                Id = scope.Id,
                StartAddress = scope.AddressRelatedProperties.Start.ToString(),
                EndAddress = scope.AddressRelatedProperties.End.ToString(),
                Name = scope.Name,
            };

            collection.Add(node);

            if (scope.GetChildScopes().Any())
            {
                foreach (var item in scope.GetChildScopes())
                {
                    GenerateScopeList(item, collection);
                }
            }
        }

        [HttpGet("/api/scopes/dhcpv6/list")]
        public IActionResult GetScopesAsList()
        {
            List<DHCPv6ScopeItem> result = new List<DHCPv6ScopeItem>();

            foreach (var item in _rootScope.GetRootScopes())
            {
                GenerateScopeList(item, result);
            }

            return base.Ok(result);
        }

        [HttpGet("/api/scopes/dhcpv6/resolvers/description")]
        public IActionResult GetResolverDescription()
        {
            var descriptions = _resolverManager.GetRegisterResolverDescription();
            return base.Ok(descriptions);
        }

        private DHCPv6ScopePropertyResponse GetScopePropertyResponse(DHCPv6ScopeProperty property)
        {
            return property switch
            {
                DHCPv6AddressListScopeProperty item => new DHCPv6AddressListScopePropertyResponse
                {
                    Addresses = item.Addresses.Select(x => x.ToString()).ToList(),
                    OptionCode = item.OptionIdentifier,
                    Type = item.ValueType,
                },
                DHCPv6NumericValueScopeProperty item => new DHCPv6NumericScopePropertyResponse
                {
                    Value = item.Value,
                    NumericType = item.NumericType,
                    Type = item.ValueType,
                    OptionCode = item.OptionIdentifier,
                },
                _ => throw new NotImplementedException(),
            };
        }

        [HttpGet("/api/scopes/dhcpv6/{id}/properties")]
        public IActionResult GetScopeProperties([FromRoute(Name = "id")] Guid scopeId, [FromQuery] Boolean includeParents = true)
        {
            var scope = _rootScope.GetScopeById(scopeId);
            if (scope == DHCPv6Scope.NotFound)
            {
                return NotFound();
            }

            var addressProperties = scope.AddressRelatedProperties;
            var scopeProperties = scope.Properties ?? DHCPv6ScopeProperties.Empty;
            if (includeParents == true)
            {
                addressProperties = scope.GetAddressProperties();
                scopeProperties = scope.GetScopeProperties();
            }

            var response = new DHCPv6ScopePropertiesResponse
            {
                Name = scope.Name,
                Description = scope.Description,
                ParentId = scope.ParentScope == null ? new Guid?() : scope.ParentScope.Id,
                Resolver = new ScopeResolverResponse
                {
                    Typename = scope.Resolver.GetDescription().TypeName,
                    PropertiesAndValues = scope.Resolver.GetValues(),
                },
                Properties = scopeProperties.Properties.Where(x => x != null).Select(x => GetScopePropertyResponse(x)).ToArray(),
                InheritanceStopedProperties = scopeProperties.GetMarkedFromInheritanceOptionCodes().Select(x => (Int32)x).ToArray(),
                AddressRelated = new DHCPv6ScopeAddressPropertiesResponse
                {
                    AcceptDecline = addressProperties.AcceptDecline,
                    AddressAllocationStrategy = addressProperties.AddressAllocationStrategy == null ? new DHCPv6ScopeAddressPropertyReqest.AddressAllocationStrategies?() : (DHCPv6ScopeAddressPropertyReqest.AddressAllocationStrategies)addressProperties.AddressAllocationStrategy,
                    End = addressProperties.End.ToString(),
                    ExcludedAddresses = addressProperties.ExcludedAddresses.Select(x => x.ToString()).ToList(),
                    InformsAreAllowd = addressProperties.InformsAreAllowd,
                    RapitCommitEnabled = addressProperties.RapitCommitEnabled,
                    ReuseAddressIfPossible = addressProperties.ReuseAddressIfPossible,
                    Start = addressProperties.Start.ToString(),
                    SupportDirectUnicast = addressProperties.SupportDirectUnicast,
                    T1 = addressProperties.T1 != null ? addressProperties.T1.Value : new Double?(),
                    T2 = addressProperties.T2 != null ? addressProperties.T2.Value : new Double?(),
                    PreferedLifetime = addressProperties.PreferredLeaseTime,
                    ValidLifetime = addressProperties.ValidLeaseTime,
                    PrefixDelegationInfo = addressProperties.PrefixDelgationInfo == null ? null : new DHCPv6PrefixDelgationInfoResponse
                    {
                        AssingedPrefixLength = addressProperties.PrefixDelgationInfo.AssignedPrefixLength,
                        Prefix = addressProperties.PrefixDelgationInfo.Prefix.ToString(),
                        PrefixLength = addressProperties.PrefixDelgationInfo.PrefixLength
                    },
                    UseDynamicRenew = addressProperties.UseDynamicRewnewTime,
                    DynamicRenew = addressProperties.UseDynamicRewnewTime == true ? new DynamicRenewTimeReponse
                    {
                        Hours = addressProperties.DynamicRenewTime.Hour,
                        Minutes = addressProperties.DynamicRenewTime.Minutes,
                        DelayToRebound = (Int32)addressProperties.DynamicRenewTime.MinutesToRebound,
                        DelayToLifetime = (Int32)addressProperties.DynamicRenewTime.MinutesToEndOfLife,
                    } : null,
                }
            };

            return base.Ok(response);
        }

        [HttpPost("/api/scopes/dhcpv6/")]
        public async Task<IActionResult> CreateScope([FromBody] CreateOrUpdateDHCPv6ScopeRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            request.Resolver.PropertiesAndValues = DictionaryHelper.NormelizedProperties(request.Resolver.PropertiesAndValues);

            Guid? item = await _mediator.Send(new CreateDHCPv6ScopeCommand(
                request.Name, request.Description, request.ParentId, request.AddressProperties, request.Resolver, request.Properties));

            if (item.HasValue == false)
            {
                return BadRequest("unable to create scope");
            }

            return Ok(item.Value);
        }

        private async Task<IActionResult> ExecuteCommand(IRequest<Boolean> command)
        {
            Boolean result = await _mediator.Send(command);
            if (result == true)
            {
                return base.NoContent();
            }

            return BadRequest("unable to execute service operation");
        }

        [HttpPut("/api/scopes/dhcpv6/{id}")]
        public async Task<IActionResult> UpdateScope([FromBody] CreateOrUpdateDHCPv6ScopeRequest request, [FromRoute(Name = "id")] Guid scopeId)
        {
            request.Resolver.PropertiesAndValues = DictionaryHelper.NormelizedProperties(request.Resolver.PropertiesAndValues);

            var command =  new UpdateDHCPv6ScopeCommand(scopeId,
                request.Name, request.Description, request.ParentId, request.AddressProperties, request.Resolver, request.Properties);

            return await ExecuteCommand(command);
        }

        [HttpDelete("/api/scopes/dhcpv6/{id}/")]
        public async Task<IActionResult> DeleteScope([FromRoute(Name = "id")] Guid scopeId, [FromQuery] Boolean includeChildren = false)
        {
            DeleteDHCPv6ScopeCommand command = new DeleteDHCPv6ScopeCommand(scopeId, includeChildren);
            return await ExecuteCommand(command);
        }

        [HttpPut("/api/scopes/dhcpv6/changeScopeParent/{id}/{parentId?}")]
        public async Task<IActionResult> UpdateScopeParent([FromRoute(Name = "id")] Guid scoopeId, [FromRoute(Name = "parentId")] Guid? parentId)
        {
            UpdateDHCPv6ScopeParentCommand command = new (scoopeId, parentId);
            return await ExecuteCommand(command);
        }
    }
}
