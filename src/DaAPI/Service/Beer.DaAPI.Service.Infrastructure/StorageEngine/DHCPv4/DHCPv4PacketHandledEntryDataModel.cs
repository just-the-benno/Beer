using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Beer.DaAPI.Core.Scopes.DHCPv4.DHCPv4PacketHandledEvents;

namespace Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4
{
    public class DHCPv4PacketHandledEntryDataModel : IPacketHandledEntry<DHCPv4MessagesTypes>
    {
        [Key]
        public Guid Id { get; set; }

        public DHCPv4MessagesTypes RequestType { get; set; }
        public UInt16 RequestSize { get; set; }
        public String RequestDestination { get; set; }
        public String RequestSource { get; set; }
        public Byte[] RequestStream { get; set; }

        public DHCPv4MessagesTypes? ResponseType { get; set; }
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

        public String MacAddress { get; set; }
        public String ClientIdentifiier { get; set; }
        public String LeasedAddressInResponse { get; set; }
        public String RequestedAddress { get; set; }

        public DateTime Timestamp { get; set; }
        public DateTime TimestampDay { get; set; }
        public DateTime TimestampWeek { get; set; }
        public DateTime TimestampMonth { get; set; }

        public Int32 Version { get; set; }

        public DHCPv4PacketHandledEntryDataModel()
        {

        }

        public void UpgradeToVersion2()
        {
            try
            {
                DHCPv4Packet request = DHCPv4Packet.FromByteArray(RequestStream,
              new IPv4HeaderInformation(IPv4Address.FromString(RequestSource), IPv4Address.FromString(RequestDestination)));

                byte[] macAddress;
                string clientIdentifierString;
                GetMacAndClientIdentifier(request, out macAddress, out clientIdentifierString);

                ClientIdentifiier = clientIdentifierString;
                MacAddress = ByteHelper.ToString(macAddress, false);
                RequestedAddress = request.ClientIPAdress.ToString();

                if (ResponseStream != null && ResponseStream.Length > 0)
                {
                    var response = DHCPv4Packet.FromByteArray(ResponseStream,
                    new IPv4HeaderInformation(IPv4Address.FromString(ResponseSource), IPv4Address.FromString(ResponseDestination)));

                    LeasedAddressInResponse = response.YourIPAdress.ToString();
                }

            }
            catch (Exception)
            {
            }
          
            Version = 2;
        }

        public DHCPv4PacketHandledEntryDataModel(DHCPv4PacketHandledEvent e)
        {
            byte[] macAddress;
            string clientIdentifierString;
            GetMacAndClientIdentifier(e.Request, out macAddress, out clientIdentifierString);

            Version = 2;

            HandledSuccessfully = e.WasSuccessfullHandled;
            ErrorCode = e.ErrorCode;
            Id = Guid.NewGuid();

            RequestSize = e.Request.GetSize();
            RequestDestination = e.Request.Header.Destionation.ToString();
            RequestSource = e.Request.Header.Source.ToString();
            RequestStream = e.Request.GetAsStream();

            ScopeId = e.ScopeId;
            RequestType = e.Request.MessageType;
            Timestamp = e.Timestamp;

            ClientIdentifiier = clientIdentifierString;
            MacAddress = ByteHelper.ToString(macAddress, false);
            RequestedAddress = e.Request.ClientIPAdress.ToString();

            if (e.Response != null)
            {
                ResponseSize = e.Response.GetSize();
                ResponseType = e.Response.MessageType;
                ResponseDestination = e.Response.Header.Destionation.ToString();
                ResponseSource = e.Response.Header.Source.ToString();
                ResponseStream = e.Response.GetAsStream();

                LeasedAddressInResponse = e.Response.YourIPAdress.ToString();
            }

            SetTimestampDates();
        }

        private static void GetMacAndClientIdentifier(DHCPv4Packet e, out byte[] macAddress, out string clientIdentifierString)
        {
            var clientIdentifier = e.GetClientIdentifier();
            macAddress = Array.Empty<byte>();
            clientIdentifierString = String.Empty;
            if (clientIdentifier != null)
            {
                if (clientIdentifier.DUID != null)
                {
                    if (clientIdentifier.DUID is LinkLayerAddressDUID a)
                    {
                        macAddress = a.LinkLayerAddress;
                    }
                    else if (clientIdentifier.DUID is LinkLayerAddressAndTimeDUID b)
                    {
                        macAddress = b.LinkLayerAddress;
                    }
                }
                else
                {
                    clientIdentifierString = clientIdentifier.IdentifierValue;
                }
            }

            if (macAddress == null || macAddress.Length == 0)
            {
                macAddress = e.ClientHardwareAddress;
            }
        }

        public void SetTimestampDates()
        {
            TimestampDay = Timestamp.Date;
            TimestampMonth = new DateTime(Timestamp.Year, Timestamp.Month, 1);
            TimestampWeek = Timestamp.GetFirstWeekDay().AddSeconds(1);
        }
    }
}
