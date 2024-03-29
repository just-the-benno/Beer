﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Infrastructure.ServiceBus;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv4;
using System.Net.Sockets;

namespace Beer.DaAPI.Infrastructure.InterfaceEngines
{
    public class DHCPv4InterfaceEngine : DHCPInterfaceEngine<DHCPv4InterfaceEngine, DHCPv4Listener, IPv4Address, DHCPv4Server, DHCPv4Packet, DHCPv4PacketArrivedMessage, InvalidDHCPv4PacketArrivedMessage>, IDHCPv4InterfaceEngine
    {
        private readonly IDHCPv4StorageEngine _storage;

        public DHCPv4InterfaceEngine(
            IServiceBus serviceBus,
            IDHCPv4StorageEngine storage,
            ILoggerFactory loggerFactory
            ) : base(serviceBus, loggerFactory.CreateLogger<DHCPv4InterfaceEngine>(),
                (listener) => new DHCPv4Server(listener.Address, serviceBus, loggerFactory.CreateLogger<DHCPv4Server>()))
        {
            this._storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _logger.LogDebug("DHCPv4InterfaceEngine ctor. {HashValue}", GetHashCode());
        }

        public async Task<IEnumerable<DHCPv4Listener>> GetActiveListeners() => await _storage.GetDHCPv4Listener();

        protected override Boolean IsValidAddress(UnicastIPAddressInformation addressInfo) =>
            addressInfo.Address.AddressFamily == AddressFamily.InterNetwork;

        public IEnumerable<DHCPv4Listener> GetPossibleListeners() => GetPossibleListeners((nic, ipAddress) => DHCPv4Listener.FromNIC(nic, ipAddress));
    }
}
