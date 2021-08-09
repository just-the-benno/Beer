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
    [Table("TracingStreams")]
    public class TracingStreamDataModel
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Int32 SystemIdentifier { get; set; }
        public Int32 ProcedureIdentifier { get; set; }
        public DateTime? ClosedAt { get; set; }

        public Dictionary<String,String> FirstEntryData { get; set; }

        public Int32 RecordCount { get; set; }

        public Boolean HasFailed { get; set; }

        public virtual ICollection<TracingStreamEntryDataModel> Entries { get; set; }

        public TracingStreamDataModel()
        {

        }

        public TracingStreamDataModel(TracingStream stream)
        {
            Id = stream.Id;
            CreatedAt = stream.CreatedAt;
            SystemIdentifier = stream.SystemIdentifier;
            ProcedureIdentifier = stream.ProcedureIdentifier;

            FirstEntryData = new Dictionary<string, string>(stream.Record.FirstOrDefault()?.Data ?? new Dictionary<string, string>());
            RecordCount = stream.Record.Count();
        }
    }
}
