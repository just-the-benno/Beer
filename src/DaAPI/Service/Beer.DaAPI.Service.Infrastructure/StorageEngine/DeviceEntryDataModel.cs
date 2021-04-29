using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    [Table("Devices")]
    public class DeviceEntryDataModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public String Name { get; set; }

        public Byte[] MacAddress { get; set; }

        public Byte[] DUID { get; set; }
    }
}
