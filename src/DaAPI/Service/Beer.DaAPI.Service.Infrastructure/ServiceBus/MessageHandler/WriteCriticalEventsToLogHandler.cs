using Beer.DaAPI.Infrastructure.NotificationEngine;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.ServiceBus.MessageHandler
{
    public class WriteCriticalEventsToLogHandler : INotificationHandler<UnableToSentPacketMessage>
    {
        private record CriticalError(DateTime Timestamp, String Subsystem, String Message);

        private readonly ILogger<WriteCriticalEventsToLogHandler> _logger;
        private const String _filePath = "critical-errors.json";

        public WriteCriticalEventsToLogHandler(
            ILogger<WriteCriticalEventsToLogHandler> logger)
        {
        }

        private async Task Handle(String subSystem, String message, CancellationToken cancellationToken)
        {
            if (File.Exists(_filePath) == false)
            {
                using var createStream = File.Create(_filePath);
                await JsonSerializer.SerializeAsync(createStream, Array.Empty<CriticalError>(), cancellationToken: cancellationToken);
                await createStream.DisposeAsync();
            }

            using var readStream = File.Open(_filePath, FileMode.Open);
            var errors = await JsonSerializer.DeserializeAsync<List<CriticalError>>(readStream, cancellationToken: cancellationToken);
            errors.Add(new CriticalError(DateTime.UtcNow, subSystem, message));

            readStream.Position = 0;

            await JsonSerializer.SerializeAsync(readStream, errors, cancellationToken: cancellationToken);
            await readStream.DisposeAsync();
        }

        public async Task Handle(UnableToSentPacketMessage notification, CancellationToken cancellationToken) => await Handle("InterfaceEngine", "unable to send packet", cancellationToken);
    }
}
