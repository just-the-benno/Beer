using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Packets
{
    public abstract class IPHeader<TAddress> : Value
        where TAddress : IPAddress<TAddress>
    {
        public TAddress ListenerAddress { get; set; }
        public TAddress Source { get; private set; }
        public TAddress Destionation { get; private set; }

        protected IPHeader(TAddress source, TAddress destionation,TAddress listenerAddress)
        {
            Source = source;
            Destionation = destionation;
            ListenerAddress = listenerAddress;
        }
    }
}
