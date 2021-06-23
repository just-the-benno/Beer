﻿using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Service.API.Application.Commands.DHCPv6Leases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public DHCPv6LeaseController(DHCPv6RootScope rootScope, IMediator mediator, ILogger<DHCPv6LeaseController> logger)
        {
            _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
            Scope = new ScopeOverview
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
                return BadRequest("unable to delete lease");
            }

            return Ok(true);
        }
    }
}
