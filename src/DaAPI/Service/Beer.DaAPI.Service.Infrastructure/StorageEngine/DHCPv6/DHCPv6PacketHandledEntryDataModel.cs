using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Beer.DaAPI.Core.Scopes.DHCPv6.DHCPv6PacketHandledEvents;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6
{
    public class DHCPv6PacketHandledEntryDataModel : IPacketHandledEntry<DHCPv6PacketTypes>
    {
        [Key]
        public Guid Id { get; set; }

        public DHCPv6PacketTypes RequestType { get; set; }
        public UInt16 RequestSize { get; set; }
        public String RequestDestination { get; set; }
        public String RequestSource { get; set; }
        public Byte[] RequestStream { get; set; }

        public DHCPv6PacketTypes? ResponseType { get; set; }
        public UInt16? ResponseSize { get; set; }
        public String ResponseDestination { get; set; }
        public String ResponseSource { get; set; }
        public Byte[] ResponseStream { get; set; }

        public Boolean HandledSuccessfully { get; set; }
        public Int32 ErrorCode { get; set; }
        public String FilteredBy { get; set; }
        public Boolean InvalidRequest { get; set; }

        public Guid? ScopeId { get; set; }
        //public Guid? LeaseId { get; set; }

        public DateTime Timestamp { get; set; }
        public DateTime TimestampDay { get; set; }
        public DateTime TimestampWeek { get; set; }
        public DateTime TimestampMonth { get; set; }

        public String MacAddress { get; set; }
        public String LeasedAddressInResponse { get; set; }

        public String RequestedAddress { get; set; }
        public String RequestedPrefix { get; set; }
        public Byte RequestedPrefixLength { get; set; }

        public String LeasedPrefix { get; set; }
        public Byte LeasedPrefixLength { get; set; }

        public String RequestedPrefixCombined { get; set; }
        public String LeaseddPrefixCombined { get; set; }

        public int Version { get; set; }

        private (string address, string prefix, byte prefixLength,string prefixCombined) ExtractAddressAndPrefix(DHCPv6Packet packet)
        {
            string address = string.Empty, prefix = string.Empty;
            byte prefixLength = 0;

            var innerPacket = packet.GetInnerPacket();

            var nonTemporaryIaid = innerPacket.GetNonTemporaryIdentityAssocationId();
            if (nonTemporaryIaid.HasValue == true)
            {
                var option = innerPacket.GetNonTemporaryIdentiyAssocation(nonTemporaryIaid.Value);
                address = option.GetAddress().ToString();
            }

            var prefixIaId = innerPacket.GetPrefixDelegationIdentityAssocationId();
            if (prefixIaId.HasValue == true)
            {
                var option = innerPacket.GetPrefixDelegationIdentiyAssocation(prefixIaId.Value);
                var prefixContent = option.GetPrefixDelegation();
                if (prefixContent != Core.Scopes.DHCPv6.DHCPv6PrefixDelegation.None)
                {
                    prefix = prefixContent.NetworkAddress.ToString();
                    prefixLength = prefixContent.Mask.Identifier.Value;
                }
            }

            return (address, prefix, prefixLength,prefix + "/" + prefix.ToString());
        }

        private void UpgradeToVersion2(DHCPv6Packet request, DHCPv6Packet response)
        {
            var requestInnerPacket = request.GetInnerPacket();

            switch (requestInnerPacket.GetClientIdentifer())
            {
                case LinkLayerAddressDUID duid:
                    MacAddress = ByteHelper.ToString(duid.LinkLayerAddress, false);
                    break;
                case LinkLayerAddressAndTimeDUID duid:
                    MacAddress = ByteHelper.ToString(duid.LinkLayerAddress, false);
                    break;
                default:
                    break;
            }

            var requetedAddress = ExtractAddressAndPrefix(request);

            RequestedAddress = requetedAddress.address;
            RequestedPrefix = requetedAddress.prefix;
            RequestedPrefixLength = requetedAddress.prefixLength;
            RequestedPrefixCombined = requetedAddress.prefixCombined;

            if (response != null)
            {
                var responseAddress = ExtractAddressAndPrefix(request);
                LeasedAddressInResponse = responseAddress.address;
                LeasedPrefix = responseAddress.prefix;
                LeasedPrefixLength = responseAddress.prefixLength;
                LeaseddPrefixCombined = requetedAddress.prefixCombined;
            }

            Version = 2;
        }

        public void UpgradeToVersion2()
        {
            try
            {
                DHCPv6Packet request = DHCPv6Packet.FromByteArray(RequestStream,
                    new IPv6HeaderInformation(IPv6Address.FromString(RequestSource), IPv6Address.FromString(RequestDestination)));

                DHCPv6Packet response = null;

                if (ResponseStream != null)
                {
                    response = DHCPv6Packet.FromByteArray(ResponseStream,
                    new IPv6HeaderInformation(IPv6Address.FromString(ResponseSource), IPv6Address.FromString(ResponseDestination)));
                }

                UpgradeToVersion2(request, response);
            }
            catch (Exception)
            {
                Version = 2;
            }
        }

        public DHCPv6PacketHandledEntryDataModel()
        {

        }

        public DHCPv6PacketHandledEntryDataModel(DHCPv6PacketHandledEvent e)
        {
            HandledSuccessfully = e.WasSuccessfullHandled;
            ErrorCode = e.ErrorCode;
            Id = Guid.NewGuid();
            ScopeId = e.ScopeId;
            Timestamp = e.Timestamp;

            RequestSize = e.Request.GetSize();
            RequestType = e.Request.GetInnerPacket().PacketType;
            RequestSource = e.Request.Header.Source.ToString();
            RequestDestination = e.Request.Header.Destionation.ToString();
            RequestStream = e.Request.GetAsStream();

            if (e.Response != null)
            {
                ResponseSize = e.Response.GetSize();
                ResponseType = e.Response.GetInnerPacket().PacketType;
                ResponseSource = e.Response.Header.Source.ToString();
                ResponseDestination = e.Response.Header.Destionation.ToString();
                ResponseStream = e.Response.GetAsStream();
            }

            SetTimestampDates();
            UpgradeToVersion2(e.Request, e.Response);
        }

        public void SetTimestampDates()
        {
            TimestampDay = Timestamp.Date;
            TimestampMonth = new DateTime(Timestamp.Year, Timestamp.Month, 1);
            TimestampWeek = Timestamp.GetFirstWeekDay().AddSeconds(1);
        }
    }
}
