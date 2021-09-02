using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    [Table("LeaseEventEntries")]
    public class LeaseEventEntryDataModel
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid ScopeId { get; set; }
        public String Address { get; set; }
        public String EventType { get; set; }
        public String FullEventType { get; set; }
        public String EventData { get; set; }
        public Guid LeaseId { get; set; }

        public Guid PacketHandledEntryId { get; set; }
        public Boolean HasResponse { get;  set; }
    }
}
