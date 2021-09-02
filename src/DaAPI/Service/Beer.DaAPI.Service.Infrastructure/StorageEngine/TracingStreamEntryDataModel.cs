using Beer.DaAPI.Core.Tracing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    [Table("TracingStreamEntries")]
    public class TracingStreamEntryDataModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public String Identifier { get; set; }

        public DateTime Timestamp { get; set; }
        public Guid? EntityId { get; set; }

        public Dictionary<String,String> AddtionalData { get; set; }

        [ForeignKey(nameof(Stream))]
        public Guid StreamId { get; set; }

        public Int32 ResultType { get; set; }

        public virtual TracingStreamDataModel Stream { get; set; }

        public TracingStreamEntryDataModel()
        {

        }

        public TracingStreamEntryDataModel(TracingRecord record, TracingStreamDataModel streamDataModel)
        {
            Id = Guid.NewGuid();

            Identifier = record.Identifier;
            Timestamp = record.Timestamp;
            EntityId = record.EntityId;

            Stream = streamDataModel;

            AddtionalData = new Dictionary<string, string>(record.Data ?? new Dictionary<string, string>());

            ResultType = (Int32)record.Status;
        }

        public TracingStreamEntryDataModel(TracingStream stream, TracingStreamDataModel streamDataModel) : this(stream.Record.First(), streamDataModel)
        {
        }
    }
}
