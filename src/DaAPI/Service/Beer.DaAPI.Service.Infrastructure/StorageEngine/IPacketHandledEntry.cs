using Beer.DaAPI.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    public interface IPacketHandledEntry<TPacketType> where TPacketType : struct
    {
        public TPacketType RequestType { get; set; }
        public UInt16 RequestSize { get; set; }
        public String RequestDestination { get; set; }
        public String RequestSource { get; set; }
        public Byte[] RequestStream { get; set; }

        public TPacketType? ResponseType { get; set; }
        public UInt16? ResponseSize { get; set; }
        public String ResponseDestination { get; set; }
        public String ResponseSource { get; set; }
        public Byte[] ResponseStream { get; set; }

        public String MacAddress { get; set; }
        public String LeasedAddressInResponse { get; set; }
        public String RequestedAddress { get; set; }

        public Int32 Version { get; set; }

        Boolean HandledSuccessfully { get; set; }
        Int32 ErrorCode { get; set; }
        String FilteredBy { get; set; }
        Boolean InvalidRequest { get; set; }

        Guid? ScopeId { get; set; }
        //public Guid? LeaseId { get; set; }

        DateTime Timestamp { get; set; }
        DateTime TimestampDay { get; set; }
        DateTime TimestampWeek { get; set; }
        DateTime TimestampMonth { get; set; }
    }
}
