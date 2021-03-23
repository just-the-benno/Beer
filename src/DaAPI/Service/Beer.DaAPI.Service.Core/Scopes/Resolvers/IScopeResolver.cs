using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Scopes
{
    public interface IScopeResolver<TPacket, TAddress> 
        where TPacket: DHCPPacket<TPacket, TAddress> 
        where TAddress : IPAddress<TAddress>
    {
        //Boolean ForceReuseOfAddress { get; }
        Boolean HasUniqueIdentifier { get; }
        Boolean PacketMeetsCondition(TPacket packet);
        Byte[] GetUniqueIdentifier(TPacket packet);
        Boolean ArePropertiesAndValuesValid(IDictionary<String, String> valueMapper, ISerializer serializer);
        void ApplyValues(IDictionary<String, String> valueMapper, ISerializer serializer);
        ScopeResolverDescription GetDescription();
        IDictionary<String, String> GetValues();
    }
}
