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
            ITracingRecord firstRecord,
            Func<TracingRecord, Boolean, Task<Boolean>> appendCallback)
        {
            SystemIdentifier = systemIdentifier;
            ProcedureIdentifier = procedureIdentfier;
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;

            _appendCallback = appendCallback;

            _level = $"{systemIdentifier}.{procedureIdentfier}";

            _records = new List<TracingRecord>();
            _records.Add(new TracingRecord(Id, _level, TracingRecordStatus.Informative, firstRecord));
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

        public async Task<Boolean> Append(Int32 eventIdenfifier, TracingRecordStatus status, ITracingRecord input) => await Append(new TracingRecord(this.Id, $"{_level}.{eventIdenfifier}", status, input));
        public async Task<Boolean> Append(Int32 eventIdenfifier, TracingRecordStatus status, IDictionary<String, String> data) => await Append(new TracingRecord(this.Id, $"{_level}.{eventIdenfifier}", data, status, _entityId));
        public async Task<Boolean> Append(Int32 eventIdenfifier, TracingRecordStatus status, IDictionary<String, String> data, Guid entityId) => await Append(new TracingRecord(this.Id, $"{_level}.{eventIdenfifier}", data, status, entityId));
        public async Task<Boolean> Append(Int32 eventIdenfifier, TracingRecordStatus status) => await Append(eventIdenfifier, status, new Dictionary<String, String>());

        private async Task<Boolean> Append(TracingRecord record) => await Append(record, false);

        public async Task<Boolean> AppendAndClose(Int32 eventIdenfifier, ITracingRecord input) => await AppendAndClose(new TracingRecord(this.Id, $"{_level}.{eventIdenfifier}", TracingRecordStatus.Informative, input));

        public void SetEntityId(Guid id) => _entityId = id;

        private async Task<Boolean> AppendAndClose(TracingRecord record) => await Append(record, true);

        private Guid? _entityId;

        public void OpenNextLevel(Int32 identifier) => _level = $"{_level}.{identifier}";
        public void RevertLevel() => _level = _level.Substring(0, _level.LastIndexOf('.'));
        public void ClearEntity() => _entityId = null;
    }
}
