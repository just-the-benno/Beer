using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public class DHCPv6LeaseEntryDataModel : ILeaseEntry
    {
        public Guid Id { get; set; }
        public Guid LeaseId { get; set; }
        public String Address { get; set; }
        public String Prefix { get; set; }
        public Byte PrefixLength { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid ScopeId { get; set; }
        public ReasonToEndLease EndReason { get; set; }
        public DateTime Timestamp { get; set; }
        public Boolean IsActive { get; set; }
        public DateTime EndOfRenewalTime { get; set; }
        public DateTime EndOfPreferredLifetime { get; set; }
        public Byte[] ClientIdentifier { get; set; }
        public Byte[] UniqueIdentifier { get; set; }
        public UInt32 IdentityAssocationId { get;  set; }
        public UInt32 IdentityAssocationIdForPrefix { get;  set; }
    }
}
