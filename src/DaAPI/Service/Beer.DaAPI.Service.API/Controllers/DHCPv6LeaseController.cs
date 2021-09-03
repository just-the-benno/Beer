using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv6Leases;
using Beer.DaAPI.Shared.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.CommenRequests.V1;
using static Beer.DaAPI.Shared.Responses.CommenResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;

namespace Beer.DaAPI.Service.API.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv6LeaseController : ControllerBase
    {
        private readonly DHCPv6RootScope _rootScope;
        private readonly IMediator _mediator;
        private readonly ILogger<DHCPv6LeaseController> _logger;
        private readonly IDHCPv6ReadStore _readStore;

        public DHCPv6LeaseController(DHCPv6RootScope rootScope, IMediator mediator, IDHCPv6ReadStore readStore, ILogger<DHCPv6LeaseController> logger)
        {
            _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private DHCPv6LeaseOverview GetLeaseOverview(DHCPv6Lease lease, DHCPv6Scope scope) => new DHCPv6LeaseOverview
        {
            Address = lease.Address.ToString(),
            ClientIdentifier = lease.ClientDUID,
            Started = lease.Start,
            ExpectedEnd = lease.End,
            Id = lease.Id,
            Prefix = lease.PrefixDelegation != DHCPv6PrefixDelegation.None ? new PrefixOverview { Address = lease.PrefixDelegation.NetworkAddress.ToString(), Mask = lease.PrefixDelegation.Mask.Identifier } : null,
            UniqueIdentifier = lease.UniqueIdentifier,
            State = lease.State,
            Scope = new()
            {
                Id = scope.Id,
                Name = scope.Name,
            }
        };

        private void GetAllLesesRecursivly(ICollection<DHCPv6LeaseOverview> collection, DHCPv6Scope scope)
        {
            var items = scope.Leases.GetAllLeases().Select(x => GetLeaseOverview(x, scope)).ToList();
            foreach (var item in items)
            {
                collection.Add(item);
            }

            var children = scope.GetChildScopes();
            if (children.Any() == false)
            {
                return;
            }

            foreach (var item in children)
            {
                GetAllLesesRecursivly(collection, item);
            }
        }

        [HttpGet("/api/leases/dhcpv6/scopes/{id}")]
        public IActionResult GetLeasesByScope([FromRoute(Name = "id")] Guid scopeId, [FromQuery(Name = "includeChildren")] Boolean includeChildren = false)
        {
            _logger.LogDebug("GetLeasesByScope");

            var scope = _rootScope.GetScopeById(scopeId);
            if (scope == DHCPv6Scope.NotFound)
            {
                return NotFound($"no scope with id {scopeId} found");
            }
            List<DHCPv6LeaseOverview> result;
            if (includeChildren == false)
            {
                result = scope.Leases.GetAllLeases().Select(x => GetLeaseOverview(x, scope)).ToList();
            }
            else
            {
                result = new List<DHCPv6LeaseOverview>();
                GetAllLesesRecursivly(result, scope);
            }

            return base.Ok(result.OrderBy(x => x.State).ThenBy(x => x.Address).ToList());
        }

        [HttpDelete("/api/leases/dhcpv6/{id}")]
        public async Task<IActionResult> CancelLease([FromRoute(Name = "id")] Guid leaseId)
        {
            var command = new CancelDHCPv6LeaseCommand(leaseId);
            Boolean result = await _mediator.Send(command);

            if (result == false)
            {
                return BadRequest("una ble to delete lease");
            }

            return Ok(true);
        }

        [HttpGet("/api/leases/dhcpv6/events")]
        public async Task<IActionResult> GetLeaseEvents([FromQuery] FilterLeaseHistoryRequest filter)
        {
            List<Guid> scopeIds = new List<Guid>();
            if (filter.ScopeId.HasValue == true)
            {
                scopeIds.Add(filter.ScopeId.Value);

                if (filter.IncludeChildren == true)
                {
                    scopeIds.AddRange(_rootScope.GetAllChildScopeIds(filter.ScopeId.Value));
                }
            }

            if (String.IsNullOrEmpty(filter.Address) == false)
            {
                try
                {
                    filter.Address = IPv6Address.FromString(filter.Address).ToString();
                }
                catch (Exception)
                {
                }
            }

            FilteredResult<LeaseEventOverview> result = await _readStore.GetDHCPv6LeaseEvents(
                filter.StartTime, filter.EndTime, filter.Address, scopeIds, filter.Start, filter.Amount);

            Dictionary<Guid, String> nameMapper = new();

            foreach (var item in result.Result)
            {
                if (nameMapper.ContainsKey(item.Scope.Id) == false)
                {
                    var scope = _rootScope.GetScopeById(item.Scope.Id);
                    nameMapper.Add(scope.Id, scope?.Name);
                }

                item.Scope.Name = nameMapper[item.Scope.Id];
            }

            return Ok(result);

        }
    }
}
