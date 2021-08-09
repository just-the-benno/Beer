using Beer.DaAPI.Core.Tracing;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Beer.DaAPI.Infrastructure.StorageEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.Tracing
{
    public class TracingManager : ITracingManager
    {
        private readonly IServiceBus _serviceBus;
        private readonly IReadStore _store;

        public TracingManager(IServiceBus serviceBus, IReadStore store)
        {
            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<Boolean> SaveTracingEntry(TracingRecord record, Boolean streamClosed)
        {
            await _serviceBus.Publish(new TracingRecordAppended(record,streamClosed));
            await _store.AddTracingRecord(record);
            if(streamClosed == true)
            {
                await _store.CloseTracingStream(record.StreamId);
            }

            return true;
        }

        public async Task<TracingStream> NewTrace(int systemIdentifier, int procedureIdentfier, ITracingRecord firstRecord)
        {
            var stream = new TracingStream(systemIdentifier, procedureIdentfier, firstRecord, SaveTracingEntry);
            await _store.AddTracingStream(stream);
            await _serviceBus.Publish(new TracingStreamStartedMessage(stream));

            return stream;
        }
    }

    public static class TracingManagerConstants
    {
        public static class Modules
        {
            public static Int32 NotificationEngine => 10;
        }

        public static class NotifcationEngineSubModels
        {
            public static Int32 HandleTriggerStarted => 1;
            public static Int32 PipelineCanHandleTrigger => 3;
            public static Int32 PipelineCanNotHandleTrigger => 4;
            public static Int32 TriggerHandled => 5;
            public static Int32 ExecutionPipelineStarted => 6;

        }
    }
}
