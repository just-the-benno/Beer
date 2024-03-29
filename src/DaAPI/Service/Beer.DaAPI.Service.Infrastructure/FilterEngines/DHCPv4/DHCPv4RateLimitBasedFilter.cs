﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Infrastructure.FilterEngines.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.FilterEngines.DHCPv4
{
    public class DHCPv4RateLimitBasedFilter : RateLimitBasedFilter<IPv4Address>, IDHCPv4PacketFilter
    {
        #region Fields

        #endregion

        #region Properties

        public UInt16 PacketsPerSecons { get; set; } = 5;

        #endregion

        #region Constructor

        public DHCPv4RateLimitBasedFilter()
        {
        }

        #endregion

        #region Methods

        public Task<Boolean> ShouldPacketBeFiltered(DHCPv4Packet packet)
        {
            Boolean result = base.FilterByRateLimit(packet.Header.Source, PacketsPerSecons);
            return Task.FromResult(result);
        }

        #endregion
    }
}
