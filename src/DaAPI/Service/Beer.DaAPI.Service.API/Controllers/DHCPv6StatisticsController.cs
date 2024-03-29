﻿using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.NotificationEngine;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.StatisticsControllerRequests.V1;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace Beer.DaAPI.Service.API.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DHCPv6StatisticsController : ControllerBase
    {
        private readonly IDHCPv6ReadStore _storage;
        private readonly DHCPv6RootScope _rootScope;

        public DHCPv6StatisticsController(
            DHCPv6RootScope rootScope,
            IDHCPv6ReadStore storage)
        {
            _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [HttpGet("/api/Statistics/HandledDHCPv6Packet/{id}")]
        public async Task<IActionResult> GetHandledDHCPv6PacketByScopeId([FromRoute(Name = "id")]Guid scopeId,[Range(1,1000)][FromQuery]Int32 amount = 100)
        {
            if(_rootScope.GetScopeById(scopeId) == DHCPv6Scope.NotFound)
            {
                return NotFound("scope not found");
            }

            var entries = await _storage.GetHandledDHCPv6PacketByScopeId(scopeId,amount);
            return base.Ok(entries);
        }

        [HttpGet("/api/Statistics/IncomingDHCPv6PacketTypes")]
        public async Task<IActionResult> GetIncomingDHCPv6PacketTypes([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetIncomingDHCPv6PacketTypes(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/FileredDHCPv6Packets")]
        public async Task<IActionResult> GetFileredDHCPv6Packets([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetFileredDHCPv6Packets(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/ErrorDHCPv6Packets")]
        public async Task<IActionResult> GetErrorDHCPv6Packets([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetErrorDHCPv6Packets(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/IncomingDHCPv6Packets")]
        public async Task<IActionResult> GetIncomingDHCPv6PacketAmount([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetIncomingDHCPv6PacketAmount(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/ActiveDHCPv6Leases")]
        public async Task<IActionResult> GetActiveDHCPv6Leases([FromQuery] GroupedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetActiveDHCPv6Leases(request.Start, request.End, request.GroupbBy);
            return base.Ok(response);
        }

        [HttpGet("/api/Statistics/ErrorCodesPerDHCPV6RequestType")]
        public async Task<IActionResult> GetErrorCodesPerDHCPV6RequestType([FromQuery] DHCPv6PacketTypeBasedTimeSeriesFilterRequest request)
        {
            var response = await _storage.GetErrorCodesPerDHCPV6RequestType(request.Start, request.End, request.PacketType);
            return base.Ok(response);
        }
    }
}
