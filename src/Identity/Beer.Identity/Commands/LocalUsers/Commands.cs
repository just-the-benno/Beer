using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.Identity.Commands.LocalUsers
{
    public record CreateLocalUserCommand(String Username, String DisplayName, String Password, String ProfilePictureUrl) : IRequest<String>;
    public record DeleteLocalUserCommand(String UserId) : IRequest<Boolean>;
    public record ResetLocalUserPasswordCommand(String UserId, String Password) : IRequest<Boolean>;
}
