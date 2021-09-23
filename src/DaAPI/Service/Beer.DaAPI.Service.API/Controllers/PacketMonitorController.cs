using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Shared.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Commands.PacketMonitorRequest.V1;
using static Beer.DaAPI.Shared.Responses.DeviceResponses.V1;

namespace Beer.DaAPI.Service.API.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class PacketMonitorController : ControllerBase
    {
        private readonly IDHCPv6ReadStore _store;
        private readonly DHCPv4RootScope _dhcpv4Rootscope;
        private readonly DHCPv6RootScope _dhcpv6Rootscope;
        private readonly ILogger<PacketMonitorController> _logger;

        public PacketMonitorController(
            IDHCPv6ReadStore store,
            DHCPv4RootScope dhcpv4Rootscope,
            DHCPv6RootScope dhcpv6Rootscope,
            ILogger<PacketMonitorController> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            this._dhcpv4Rootscope = dhcpv4Rootscope;
            this._dhcpv6Rootscope = dhcpv6Rootscope;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/api/PacketMonitor/DHCPv4")]
        public async Task<IActionResult> GetDHCPv4Packet([FromQuery] DHCPv4PacketFilter filter)
        {
            _logger.LogDebug("GetDHCPv4Packet");

            filter ??= new DHCPv4PacketFilter();

            var result = await _store.GetDHCPv4Packet(filter);
            foreach (var item in result.Result.Where(x => x.Scope != null))
            {
                var scope = _dhcpv4Rootscope.GetScopeById(item.Scope.Id);
                if (scope != null)
                {
                    item.Scope.Name = scope.Name?.Value;
                }
            }

            return base.Ok(result);
        }

        private String FormatPrefix(String input)
        {
            var prefixPart = String.Empty;
            var addressPart = input;
            Int32 prefixLengthIndex = input.IndexOf('/');
            if (prefixLengthIndex >= 0)
            {
                prefixPart = input.Substring(prefixLengthIndex);
                if (prefixLengthIndex != 0)
                {
                    addressPart = input.Substring(0, prefixLengthIndex);
                }
            }

            if (String.IsNullOrEmpty(addressPart) == true)
            {
                try
                {
                    IPv6Address address = IPv6Address.FromString(addressPart);
                    addressPart = address.ToString();
                }
                catch (Exception)
                {
                }
            }

            input = $"{addressPart}{prefixPart}";
            return input;
        }

        private String FormatIPv6Address(String input)
        {
            var result = input;
            try
            {
                IPv6Address address = IPv6Address.FromString(input);
                result = address.ToString();
            }
            catch (Exception)
            {
            }

            return result;
        }

        [HttpGet("/api/PacketMonitor/DHCPv6")]
        public async Task<IActionResult> GetDHCPv6Packet([FromQuery] DHCPv6PacketFilter filter)
        {
            _logger.LogDebug("GetDHCPv4Packet");

            filter ??= new DHCPv6PacketFilter();

            if (String.IsNullOrEmpty(filter.SourceAddress) == false)
            {
                filter.SourceAddress = FormatIPv6Address(filter.SourceAddress);
            }

            if (String.IsNullOrEmpty(filter.DestinationAddress) == false)
            {
                filter.DestinationAddress = FormatIPv6Address(filter.DestinationAddress);
            }

            if (String.IsNullOrEmpty(filter.RequestedPrefix) == false)
            {
                filter.RequestedPrefix = FormatPrefix(filter.RequestedPrefix);
            }

            if (String.IsNullOrEmpty(filter.LeasedPrefix) == false)
            {
                filter.LeasedPrefix = FormatPrefix(filter.LeasedPrefix);
            }

            var result = await _store.GetDHCPv6Packet(filter);
            foreach (var item in result.Result.Where(x => x.Scope != null))
            {
                var scope = _dhcpv6Rootscope.GetScopeById(item.Scope.Id);
                if (scope != null)
                {
                    item.Scope.Name = scope.Name?.Value;
                }
            }

            return base.Ok(result);
        }

        [HttpGet("/api/PacketMonitor/Requests/DHCPv6/{id}")]
        public async Task<IActionResult> GetDHCPv6PacketRequest([FromRoute(Name = "id")] Guid packetEnrtyId)
        {
            var result = await _store.GetDHCPv6PacketRequestDataById(packetEnrtyId);
            return base.Ok(result);
        }

        [HttpGet("/api/PacketMonitor/Responses/DHCPv6/{id}")]
        public async Task<IActionResult> GetDHCPv6PacketResponse([FromRoute(Name = "id")] Guid packetEnrtyId)
        {
            var result = await _store.GetDHCPv6PacketResponseDataById(packetEnrtyId);
            return base.Ok(result);
        }

        [HttpGet("/api/PacketMonitor/Requests/DHCPv4/{id}")]
        public async Task<IActionResult> GetDHCPv4PacketRequest([FromRoute(Name = "id")] Guid packetEnrtyId)
        {
            var result = await _store.GetDHCPv4PacketRequestDataById(packetEnrtyId);
            return base.Ok(result);
        }

        [HttpGet("/api/PacketMonitor/Responses/DHCPv4/{id}")]
        public async Task<IActionResult> GetDHCPv4PacketResponse([FromRoute(Name = "id")] Guid packetEnrtyId)
        {
            var result = await _store.GetDHCPv4PacketResponseDataById(packetEnrtyId);
            return base.Ok(result);
        }

        [HttpGet("/api/PacketMonitor/InAndOut/{id}")]
        public async Task<IActionResult> GetInAndOutgoingPackets([FromRoute(Name = "id")] Guid scopeId,[FromQuery(Name = "referenceTime")] DateTime? referenceTime)
        {
            if (referenceTime.HasValue == false)
            {
                referenceTime = DateTime.UtcNow;
            }

            var result = await _store.GetIncomingAndOutgoingPacketAmount(scopeId, referenceTime.Value);
            return base.Ok(result);
        }
    }
}
