using Beer.DaAPI.Infrastructure.StorageEngine;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Shared.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.TracingRequests.V1;
using static Beer.DaAPI.Shared.Responses.DeviceResponses.V1;

namespace Beer.DaAPI.Service.API.ApiControllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "DaAPI-API")]
    [ApiController]
    public class TracingController : ControllerBase
    {
        private readonly IReadStore _store;
        private readonly ILogger<TracingController> _logger;

        public TracingController(
            IReadStore store,
            ILogger<TracingController> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("/api/tracing/streams")]
        public async Task<IActionResult> GetAllStreams([FromQuery] FilterTracingRequest request)
        {
            _logger.LogDebug("GetAllStreams");

            request ??= new FilterTracingRequest();
            if (request.Amount > 500)
            {
                request.Amount = 500;
            }
            if (request.Start < 0)
            {
                request.Start = 0;
            }

            var storeResult = await _store.GetTracingOverview(request);
            return base.Ok(storeResult);
        }

        [HttpGet("/api/tracing/streams/{id}/records/{entityId?}")]
        public async Task<IActionResult> GetStreamRecords([FromRoute(Name = "id")] Guid tracingStreamId, [FromRoute(Name = "entityId")] Guid entitiyId)
        {
            _logger.LogDebug("GetAllStreams");

            var storeResult = await _store.GetTracingStreamRecords(tracingStreamId, entitiyId);
            return base.Ok(storeResult);
        }
    }
}
