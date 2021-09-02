using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Shared.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Hubs.Tracing
{
    public class TracingStreamStartedMessageHandler : INotificationHandler<TracingStreamStartedMessage>
    {
        private readonly IHubContext<TracingHub, ITracingClient> _hubContext;

        public TracingStreamStartedMessageHandler(IHubContext<TracingHub, ITracingClient> hubContext)
        {
            this._hubContext = hubContext;
        }

        public async Task Handle(TracingStreamStartedMessage notification, CancellationToken cancellationToken)
        {
            var stream = notification.Stream;
            var firstRecord = stream.Record.FirstOrDefault();

            var groups = stream.GetHubGroups();

            var clients =  _hubContext.Clients.Groups(groups);

            await clients.StreamStarted(new Shared.Responses.TracingResponses.V1.TracingStreamOverview
            {
                FirstEntryData = firstRecord != null ? firstRecord.Data : new Dictionary<String, String>(),
                IsInProgress = true,
                ModuleIdentifier = stream.SystemIdentifier,
                ProcedureIdentifier = stream.ProcedureIdentifier,
                RecordAmount = stream.Record.Count(),
                Status = Shared.Responses.TracingResponses.V1.TracingRecordStatusForResponses.Informative,
                Id = stream.Id,
                Timestamp = stream.CreatedAt,
            });
        }
    }
}
