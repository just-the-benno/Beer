using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands.DHCPv6Leases
{
    public record CancelDHCPv6LeaseCommand(Guid LeaseId) : IRequest<Boolean>;
}
