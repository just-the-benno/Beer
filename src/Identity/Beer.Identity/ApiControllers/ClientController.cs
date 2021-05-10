using Beer.Identity.Commands.Clients;
using Beer.Identity.Commands.LocalUsers;
using Beer.Identity.Infrastructure.Repositories;
using Beer.Identity.Services;
using Beer.Identity.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.Identity.Shared.Requests.ClientRequests.V1;

namespace Beer.Identity.ApiControllers
{
    [Authorize(AuthenticationSchemes = AuthenticationDefaults.BearerSchemaName, Policy = AuthenticationDefaults.ClientPolicyName)]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IClientRepository _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(
            IMediator mediator,
            IClientRepository clientService,
            ILogger<ClientController> logger
            )
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _clientService = clientService;
            _logger = logger;
        }

        [HttpGet("/api/Clients")]
        public async Task<IActionResult> GetAllClients()
        {
            _logger.LogDebug("GetAllClients");

            var result = await _clientService.GetAllClientsSortedByName();
            return base.Ok(result);
        }

        private async Task<IActionResult> ExecuteCommand(IRequest<Boolean> command)
        {
            Boolean result = await _mediator.Send(command);
            if (result == false)
            {
                return BadRequest("unable to complete service operation");
            }

            return NoContent();
        }

        [HttpDelete("/api/Clients/{id}")]
        public async Task<IActionResult> DeleteClient(
        [FromRoute(Name = "id")] Guid systemId)
        {
            return await ExecuteCommand(new DeleteClientCommand(systemId));
        }

        [HttpPost("/api/Clients/")]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            Guid? systemid = await _mediator.Send(new CreateClientCommand(request));
            if (systemid.HasValue == true)
            {
                return base.Ok(systemid.Value);
            }
            else
            {
                return BadRequest("unable to complete service operation");
            }
        }

        [HttpPut("/api/Clients/{id}")]
        public async Task<IActionResult> UpdateClient([FromRoute(Name = "id")] Guid systemId, [FromBody] UpdateClientRequest request)
        {
            request.SystemId = systemId;

            return await ExecuteCommand(new UpdateClientCommand(request));
        }
    }
}
