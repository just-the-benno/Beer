using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Application.Commands.Notifications
{
    public class DeleteNotificationPipelineCommand : IRequest<Boolean>
    {
        public Guid PipelineId { get; }

        public DeleteNotificationPipelineCommand(Guid pipelineId)
        {
            PipelineId = pipelineId;
        }

    }
}
