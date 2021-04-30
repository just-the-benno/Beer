using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Shared.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DeviceResponses.V1;

namespace Beer.DaAPI.Service.API.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDHCPv6ReadStore _store;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(
            IDHCPv6ReadStore store,
            ILogger<DeviceController> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/api/devices/")]
        public IActionResult GetAllDevices()
        {
            _logger.LogDebug("GetAllDevices");

            var storeResult = _store.GetAllDevices();
            var result = storeResult.Select(x => new DeviceOverviewResponse
            {
                Name = x.Name,
                Id = x.Id,
            }).ToList();

            return base.Ok(result);
        }
    }
}
