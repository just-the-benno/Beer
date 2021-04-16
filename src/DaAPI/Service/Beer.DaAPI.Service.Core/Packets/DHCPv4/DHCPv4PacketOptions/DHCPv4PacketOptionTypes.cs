﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Packets.DHCPv4
{
    public enum DHCPv4OptionTypes : Byte
    {
        SubnetMask = 1,
        TimeOffset = 2,
        Router = 3,
        TimeServer = 4,
        NameServer = 5,
        DNSServers = 6,
        LogServers = 7,
        CookieServer = 8,
        LRPServers = 9,
        ImpressSever = 10,
        ResourceLocationServer = 11,
        Hostname = 12,
        BootFileSize = 13,
        MaritDumpFile = 14,
        DomainName = 15,
        SwapServer = 16,
        RootPath = 17,
        ExtentionsPath = 18,
        IpForwardingEnabled = 19,
        NonLocalSourceRouting = 20,
        PolicyFilter = 21,
        MaximumDatagramReassembly = 22,
        DefaultTTL = 23,
        PathMTUAgingTimeout = 24,
        PathMTUPlateauTable = 25,
        InterfaceMTU = 26,
        AllSubnetsAreLocal = 27,
        BroadcastAddress = 28,
        PerformMaskDiscovery = 29,
        MaskSupplier = 30,
        PerformRouterDiscovery = 31,
        RouterSolicitationAddress = 32,
        StaticRoute = 33,
        TrailerEncapsulation = 34,
        ARPCacheTimeout = 35,
        EthernetEncapsulation = 36,
        TCPDefaultTTL = 37,
        TCPKeepaliveInterval = 38,
        TCPKeepaliveGarbage = 39,
        NetworkInformationServiceDomain = 40,
        NetworkInformationServers = 41,
        NetworkTimeProtocolServers = 42,
        VendorSpecificInformation = 43,
        NetBIOSoverTCP_IPNameServer = 44,
        NetBIOSoverTCP_IPDatagramDistributionServer = 45,
        NetBIOSoverTCP_IPNodeType = 46,
        NetBIOSoverTCP_IPScope = 47,
        XWindowSystemFontServer = 48,
        XWindowSystemDisplayManager = 49,
        NetworkInformationServicePlusDomain = 64,
        NetworkInformationServicePlusServers = 65,
        MobileIPHomeAgent = 68,
        SMTPServers = 69,
        POP3Servers = 70,
        NetworkNewsTransportProtocolServer = 71,
        WWWServers = 72,
        DefaultFingerServer = 73,
        IRCServer = 74,
        StreetTalkServer = 75,
        StreetTalkDirectoryAssistance = 76,
        RequestedIPAddress = 50,
        IPAddressLeaseTime = 51,
        OptionOverload = 52,
        TFTPServername = 66,
        BootfileName = 67,
        MessageType = 53,
        ServerIdentifier = 54,
        ParamterRequestList = 55,
        Message = 56,
        MaximumDHCPMessageSize = 57,
        RenewalTimeValue = 58,
        RebindingTimeValue = 59,
        VendorClassIdentifier = 60,
        ClientIdentifier = 61,
        Option82 = 82,
    }
}
