using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Scopes
{
    public interface IScopeResolverContainingOtherResolvers<TPacket,TAddress> : IScopeResolver<TPacket,TAddress>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>
    {
        Boolean AddResolver(IScopeResolver<TPacket, TAddress> resolver);
        IEnumerable<CreateScopeResolverInformation> ExtractResolverCreateModels(CreateScopeResolverInformation item, ISerializer serializer);
    }
}
