using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv4Leases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4LeasesResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6LeasesResponses.V1;

namespace Beer.DaAPI.Service.API.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv4LeaseController : ControllerBase
    {
        private readonly DHCPv4RootScope _rootScope;
        private readonly ILogger<DHCPv4LeaseController> _logger;
        private readonly IMediator _mediator;

        public DHCPv4LeaseController(DHCPv4RootScope rootScope, IMediator mediator, ILogger<DHCPv4LeaseController> logger)
        {
            _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private DHCPv4LeaseOverview GetLeaseOverview(DHCPv4Lease lease, DHCPv4Scope scope) => new DHCPv4LeaseOverview
        {
            Address = lease.Address.ToString(),
            MacAddress = lease.Identifier.HwAddress,
            Started = lease.Start,
            ExpectedEnd = lease.End,
            Id = lease.Id,
            UniqueIdentifier = lease.UniqueIdentifier,
            State = lease.State,
            Scope = new DHCPv4ScopeOverview
            {
                Id = scope.Id,
                Name = scope.Name,
            }
        };

        private void GetAllLesesRecursivly(ICollection<DHCPv4LeaseOverview> collection, DHCPv4Scope scope)
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

        [HttpGet("/api/leases/dhcpv4/scopes/{id}")]
        public IActionResult GetLeasesByScope([FromRoute(Name = "id")] Guid scopeId, [FromQuery(Name = "includeChildren")] Boolean includeChildren = false)
        {
            _logger.LogDebug("GetLeasesByScope");

            var scope = _rootScope.GetScopeById(scopeId);
            if (scope == DHCPv4Scope.NotFound)
            {
                return NotFound($"no scope with id {scopeId} found");
            }
            List<DHCPv4LeaseOverview> result;
            if (includeChildren == false)
            {
                result = scope.Leases.GetAllLeases().Select(x => GetLeaseOverview(x, scope)).ToList();
            }
            else
            {
                result = new List<DHCPv4LeaseOverview>();
                GetAllLesesRecursivly(result, scope);
            }

            return base.Ok(result.OrderBy(x => x.State).ThenBy(x => x.Address).ToList());
        }

        [HttpDelete("/api/leases/dhcpv4/{id}")]
        public async Task<IActionResult> CancelLease([FromRoute(Name = "id")] Guid leaseId)
        {
            var command = new CancelDHCPv4LeaseCommand(leaseId);
            Boolean result = await _mediator.Send(command);

            if (result == false)
            {
                return BadRequest("unable to delete lease");
            }

            return Ok(true);
        }
    }
}
