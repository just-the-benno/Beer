using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Tracing
{
    public class TracingStream
    {
        private List<TracingRecord> _records;
        private String _level;
        private Func<TracingRecord, Boolean, Task<Boolean>> _appendCallback;

        public DateTime CreatedAt { get; init; }
        public Guid Id { get; init; }
        public Int32 SystemIdentifier { get; init; }
        public Int32 ProcedureIdentifier { get; init; }

        public IEnumerable<TracingRecord> Record => _records.AsEnumerable();


        public TracingStream(
            Int32 systemIdentifier, Int32 procedureIdentfier,
            TracingRecord firstRecord,
            Func<TracingRecord, Boolean, Task<Boolean>> appendCallback)
        {
            SystemIdentifier = systemIdentifier;
            ProcedureIdentifier = procedureIdentfier;
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;

            _appendCallback = appendCallback;

            _records = new List<TracingRecord>();
            _records.Add(firstRecord);

            _level = $"{systemIdentifier}.{procedureIdentfier}";
        }

        public async Task<Boolean> Append(TracingRecord record, Boolean close)
        {
            _records.Add(record);
            if (_appendCallback == null)
            {
                return true;
            }

            return await _appendCallback(record, close);
        }

        public async Task<Boolean> Append(Int32 eventIdenfifier, ITracingRecord input) => await Append(new TracingRecord($"{_level}.{eventIdenfifier}", input));
        public async Task<Boolean> Append(Int32 eventIdenfifier, IDictionary<String, String> data) => await Append(new TracingRecord($"{_level}.{eventIdenfifier}", data, _entityId));
        public async Task<Boolean> Append(Int32 eventIdenfifier, IDictionary<String, String> data, Guid entityId) => await Append(new TracingRecord($"{_level}.{eventIdenfifier}", data, entityId));
        public async Task<Boolean> Append(Int32 eventIdenfifier) => await Append(eventIdenfifier, new Dictionary<String, String>());

        private async Task<Boolean> Append(TracingRecord record) => await Append(record, false);

        public async Task<Boolean> AppendAndClose(Int32 eventIdenfifier, ITracingRecord input) => await AppendAndClose(new TracingRecord($"{_level}.{eventIdenfifier}", input));

        public void SetEntityId(Guid id) => _entityId = id;

        private async Task<Boolean> AppendAndClose(TracingRecord record) => await Append(record, true);

        private Guid? _entityId;

        public void OpenNextLevel(Int32 identifier) => _level = $"{_level}.{identifier}";
        public void RevertLevel() => _level = _level.Substring(0, _level.LastIndexOf('.'));
        public void ClearEntity() => _entityId = null;
    }
}
