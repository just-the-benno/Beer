using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.Identity.Shared.Requests.ClientRequests.V1;

namespace Beer.Identity.Commands.Clients
{
    public record CreateClientCommand(CreateClientRequest Request) : IRequest<Guid?>;
    public record DeleteClientCommand(Guid SystemId) : IRequest<Boolean>;
    public record UpdateClientCommand(UpdateClientRequest Request) : IRequest<Boolean>;
}
