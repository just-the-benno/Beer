using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public class DHCPv4LeaseEntryDataModel : ILeaseEntry
    {
        public Guid Id { get; set; }
        public Guid LeaseId { get; set; }
        public String Address { get; set; }
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
        public Byte[] ClientMacAddress { get; set; }
        public UInt32 OrderNumber { get; set; }


        public DHCPv4LeaseEntryDataModel()
        {

        }
    }
}
