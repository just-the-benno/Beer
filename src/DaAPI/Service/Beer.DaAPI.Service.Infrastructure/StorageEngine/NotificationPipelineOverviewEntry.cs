using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    [Table("NotificationPipelineEntries")]
    public class NotificationPipelineOverviewEntry
    {
        [Key]
        public Guid Id { get; set; }

        public String Name { get; set; }

    }
}
