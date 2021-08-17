using Beer.DaAPI.Core.Tracing;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.HostedService
{
    public class FakeTracingStreamEmitter : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceBus _serviceBus;

        private class UnusedBlub : ITracingRecord
        {
            public Guid? Id => null;

            public IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<string, string>()
            {
                { "MyProperty", "MyValue" },
            };

            public bool HasIdentity() => false;
        }

        public FakeTracingStreamEmitter(IServiceBus bus)
        {
            _serviceBus = bus;
        }

        private async void DoWork(object _)
        {
            for (int i = 0; i < 10; i++)
            {
                TracingStream stream = new(10, 5, new UnusedBlub(), null);

                await _serviceBus.Publish(new TracingStreamStartedMessage(stream));

                await Task.Delay(1000);

                for (int k = 0; k < 5; k++)
                {
                    TracingRecord record = new(stream.Id, "10.5.2", TracingRecordStatus.Informative, new UnusedBlub());
                    await _serviceBus.Publish(new TracingRecordAppendedMessage(record, true));

                    await Task.Delay(500);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer((x) => { try { DoWork(x); } catch { } }, null, TimeSpan.FromSeconds(30),
TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
