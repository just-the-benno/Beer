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
using static Beer.DaAPI.Shared.Responses.TracingResponses.V1;

namespace Beer.DaAPI.Service.API.Hubs.Tracing
{
    public class TracingRecordAppendedMessageHandler : INotificationHandler<TracingRecordAppendedMessage>
    {
        private readonly IHubContext<TracingHub, ITracingClient> _hubContext;

        public TracingRecordAppendedMessageHandler(IHubContext<TracingHub, ITracingClient> hubContext)
        {
            this._hubContext = hubContext;
        }

        public async Task Handle(TracingRecordAppendedMessage notification, CancellationToken cancellationToken)
        {
            var record = notification.Record;

            var groups = record.GetHubGroups();

            var clients = _hubContext.Clients.Groups(groups);

            await clients.RecordAppended(new TracingStreamRecord
            {
                AddtionalData = record.Data,
                EntityId = record.EntityId,
                Identifier = record.Identifier,
                Status = (TracingRecordStatusForResponses)record.Status,
                Timestamp = record.Timestamp,
            }, notification.StreamClosed, record.StreamId);
        }
    }
}
