using Beer.Identity.Commands.LocalUsers;
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
using static Beer.Identity.Shared.Requests.LocalUserRequests.V1;

namespace Beer.Identity.ApiControllers
{
    [Authorize(AuthenticationSchemes = AuthenticationDefaults.BearerSchemaName, Policy = AuthenticationDefaults.LocalUserPolicyName)]
    [ApiController]
    public class LocalUserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILocalUserService _localUserService;
        private readonly IProfilePictureService _profilePictureService;
        private readonly ILogger<LocalUserController> _logger;

        public LocalUserController(
            IMediator mediator,
            ILocalUserService localUserService,
            IProfilePictureService profilePictureService,
            ILogger<LocalUserController> logger
            )
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localUserService = localUserService;
            this._profilePictureService = profilePictureService ?? throw new ArgumentNullException(nameof(profilePictureService));
            _logger = logger;
        }

        [HttpGet("/api/LocalUsers/Initilized")]
        [AllowAnonymous]
        public async Task<IActionResult> IsLocalStoreInitilized()
        {
            _logger.LogDebug("IsLocalStoreInitilized");

            var users = await _localUserService.GetAllUsersSortedByName();
            return base.Ok(users.Any());
        }

        [HttpGet("/api/LocalUsers")]
        public async Task<IActionResult> GetAllLocalUsers()
        {
            _logger.LogDebug("GetAllLocalUsers");

            var users = await _localUserService.GetAllUsersSortedByName();
            return base.Ok(users);
        }

        [HttpGet("/api/LocalUsers/Exists/{id}")]
        public async Task<IActionResult> CheckIfUsernameExists([FromRoute(Name = "id")]String username)
        {
            _logger.LogDebug("GetAllLocalUsers");

            var result = await _localUserService.CheckIfUsernameExists(username);
            return base.Ok(result);
        }

        [HttpGet("/api/LocalUsers/Avatars/")]
        public IActionResult GetAvailableAvatars()
        {
            String requestingUrl = $"{base.Request.Scheme}://{base.Request.Host}";

            var result = _profilePictureService.GetPossibleProfilePicture().Select(x => requestingUrl + x.Url).ToList();
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

        [HttpPut("/api/LocalUsers/ChangePassword/{id}")]
        public async Task<IActionResult> ResetUserPassword(
            [FromRoute(Name = "id")] String userId, [FromBody] ResetPasswordRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            return await ExecuteCommand(new ResetLocalUserPasswordCommand(userId, request.Password));
        }

        [HttpDelete("/api/LocalUsers/{id}")]
        public async Task<IActionResult> DeleteUser(
        [FromRoute(Name = "id")] String userId)
        {
            return await ExecuteCommand(new DeleteLocalUserCommand(userId));
        }

        [HttpPost("/api/LocalUsers/")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            String id = await _mediator.Send(new CreateLocalUserCommand(request.Username, request.DisplayName, request.Password, request.ProfilePictureUrl));
            if (String.IsNullOrEmpty(id) == true)
            {
                return BadRequest("unable to complete service operation");
            }

            return base.Ok($"\"{id}\"");
        }


    }
}
