using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Tracing
{
    public enum TracingRecordStatus
    {
        Informative = 0,
        Error = 10,
        Success = 20,
    }

    public class TracingRecord
    {
        public Guid StreamId { get; init; }
        public String Identifier { get; init; }
        public IDictionary<String, String> Data { get; init; }
        public DateTime Timestamp { get; init; }
        public Guid? EntityId { get; init; }
        public TracingRecordStatus Status { get; init; }

        public TracingRecord(Guid streamId, String identifier, IDictionary<String, String> data, TracingRecordStatus status, Guid? entityId)
        {
            StreamId = streamId;
            Timestamp = DateTime.UtcNow;
            Data = data;
            Identifier = identifier;

            EntityId = entityId;
            Status = status;
        }

        public TracingRecord(Guid streamId, String identifier, TracingRecordStatus status, ITracingRecord input) : this(streamId, identifier, input.GetTracingRecordDetails(), status, input.Id)
        {
        }

        public override string ToString() => $"{Identifier} | {EntityId} | {(Data.Count == 0 ? "Empty" : JsonSerializer.Serialize(Data))}";
    }
}
