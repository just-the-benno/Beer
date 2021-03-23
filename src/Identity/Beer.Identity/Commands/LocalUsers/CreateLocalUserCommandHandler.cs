using Beer.Identity.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.Identity.Commands.LocalUsers
{
    public class CreateLocalUserCommandHandler : IRequestHandler<CreateLocalUserCommand, String>
    {
        private readonly ILocalUserService _userService;
        private readonly IProfilePictureService _profilePictureService;
        private readonly ILogger<CreateLocalUserCommandHandler> _logger;

        public CreateLocalUserCommandHandler(
            ILocalUserService userService, IProfilePictureService profilePictureService, ILogger<CreateLocalUserCommandHandler> logger)
        {
            this._userService = userService;
            this._profilePictureService = profilePictureService;
            this._logger = logger;
        }

        public async Task<String> Handle(CreateLocalUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            var possiblePictures = _profilePictureService.GetPossibleProfilePicture().Select(x => x.Url).ToHashSet();
            if(possiblePictures.Contains(request.ProfilePictureUrl) == false)
            {
                return String.Empty;
            }

            Guid? id = await _userService.CreateUser(request.Username, request.Password, request.DisplayName, request.ProfilePictureUrl);
            if (id.HasValue == false) { return null; }

            return id.ToString();
        }
    }
}
