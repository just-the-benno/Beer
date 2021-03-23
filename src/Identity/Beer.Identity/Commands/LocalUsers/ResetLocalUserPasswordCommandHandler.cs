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
    public class ResetLocalUserPasswordCommandHandler : IRequestHandler<ResetLocalUserPasswordCommand, Boolean>
    {
        private readonly ILocalUserService _userService;
        private readonly ILogger<ResetLocalUserPasswordCommandHandler> _logger;

        public ResetLocalUserPasswordCommandHandler(
            ILocalUserService userService, ILogger<ResetLocalUserPasswordCommandHandler> logger)
        {
            this._userService = userService;
            this._logger = logger;
        }

        public async Task<Boolean> Handle(ResetLocalUserPasswordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handle started");

            if (await _userService.CheckIfUserExists(request.UserId) == false)
            {
                return false;
            }

            Boolean result = await _userService.ResetPassword(request.UserId, request.Password);
            return result;
        }
    }
}
