using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv4Leases
{
    public record CancelDHCPv4LeaseCommand(Guid LeaseId) : IRequest<Boolean>;
}
