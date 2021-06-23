using Beer.DaAPI.Infrastructure.ServiceBus.MessageHandler;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Beer.DaAPI.UnitTests.Infrastructure.ServiceBus.MessageHandler
{
    public class WriteCriticalEventsToLogHandlerTester
    {
        private String _filename = "critical-errors.json";

        private class CriticalError
        {
            public DateTime Timestamp { get; set; }
            public String Subsystem { get; set; }
            public String Message { get; set; }
        }

        [Fact]
        public async Task Handle_FileNotExists()
        {
            File.Delete(_filename);
            
            WriteCriticalEventsToLogHandler handler = new (
                Mock.Of<ILogger<WriteCriticalEventsToLogHandler>>());

            await handler.Handle(new UnableToSentPacketMessage("192.168.10.5"), CancellationToken.None);

            Assert.True(File.Exists(_filename));

            using var readStream = File.OpenRead(_filename);
            var errors = await JsonSerializer.DeserializeAsync<List<CriticalError>>(readStream);

            Assert.Single(errors);

            Assert.True((DateTime.UtcNow - errors[0].Timestamp).TotalSeconds < 20);
            Assert.Equal("InterfaceEngine", errors[0].Subsystem);
            Assert.Equal("unable to send packet", errors[0].Message);
        }
    }
}
