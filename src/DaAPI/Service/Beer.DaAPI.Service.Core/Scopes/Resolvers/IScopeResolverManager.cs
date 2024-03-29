﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Core.Scopes
{
    public interface  IScopeResolverManager<TPacket, TAddress>
        where TPacket : DHCPPacket<TPacket, TAddress>
        where TAddress : IPAddress<TAddress>

    {
        Boolean IsResolverInformationValid(CreateScopeResolverInformation resolverCreateModel);
        IScopeResolver<TPacket, TAddress> InitializeResolver(CreateScopeResolverInformation resolverCreateModel);

        IEnumerable<ScopeResolverDescription> GetRegisterResolverDescription();
        void AddOrUpdateScopeResolver(String name, Func<IScopeResolver<TPacket, TAddress>> activator);
        Boolean RemoveResolver(String name);

    }
}
