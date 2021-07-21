using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Text.Json;

namespace Beer.DaAPI.Core.Notifications.Triggers
{
    public class PrefixBinding : Value<PrefixBinding>
    {
        public IPv6Address Host { get; }
        public IPv6Address Prefix { get; }
        public IPv6SubnetMask Mask { get; }

        public PrefixBinding(IPv6Address prefix, IPv6SubnetMaskIdentifier identifier, IPv6Address host)
            : this(prefix, new IPv6SubnetMask(identifier), host)
        {
        }

        public PrefixBinding(IPv6Address prefix, IPv6SubnetMask mask, IPv6Address host)
        {
            if (mask.IsIPv6AdressANetworkAddress(prefix) == false)
            {
                throw new ArgumentException("");
            }

            if (mask.IsAddressInSubnet(prefix, host) == true)
            {
                throw new ArgumentException();
            }

            Prefix = prefix;
            Mask = mask;
            Host = host;
        }

        internal static PrefixBinding FromLease(DHCPv6Lease currentLease, Boolean throwException = true)
        {
            try
            {
                return new PrefixBinding(currentLease.PrefixDelegation.NetworkAddress, currentLease.PrefixDelegation.Mask, currentLease.Address);
            }
            catch (Exception)
            {
                if(throwException == true)
                {
                    throw;
                }

                return null;
            }
        }
    }

    public class PrefixEdgeRouterBindingUpdatedTrigger : NotifcationTrigger
    {
        public PrefixBinding OldBinding { get; }
        public PrefixBinding NewBinding { get; }

        public Guid ScopeId { get; }

        internal PrefixEdgeRouterBindingUpdatedTrigger(PrefixBinding oldBinding, PrefixBinding newBinding, Guid scopeId)
        {
            OldBinding = oldBinding;
            NewBinding = newBinding;
            ScopeId = scopeId;
        }

        public override String GetTypeIdentifier() => nameof(PrefixEdgeRouterBindingUpdatedTrigger);
        public override Boolean IsEmpty() => OldBinding == null && NewBinding == null;

        public static PrefixEdgeRouterBindingUpdatedTrigger NoChanges(Guid scopeId) => new PrefixEdgeRouterBindingUpdatedTrigger(null, null, scopeId);
        public static PrefixEdgeRouterBindingUpdatedTrigger WithOldBinding(Guid scopeId, PrefixBinding oldBinding) => new PrefixEdgeRouterBindingUpdatedTrigger(oldBinding, null, scopeId);
        public static PrefixEdgeRouterBindingUpdatedTrigger WithNewBinding(Guid scopeId, PrefixBinding newBinding) => new PrefixEdgeRouterBindingUpdatedTrigger(null, newBinding, scopeId);
        public static PrefixEdgeRouterBindingUpdatedTrigger WithOldAndNewBinding(Guid scopeId, PrefixBinding oldBinding, PrefixBinding newBinding) => new PrefixEdgeRouterBindingUpdatedTrigger(oldBinding, newBinding, scopeId);

        public override IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<String, String>
        {
            { "Name", "PrefixEdgeRouterBindingUpdatedTrigger" },
            {  "OldBinding", JsonSerializer.Serialize(OldBinding == null ? null : new { Host = OldBinding.Host.ToString(), Network = OldBinding.Prefix.ToString(), Mask = OldBinding.Mask.Identifier.ToString()   }) },
            {  "NewBinding", JsonSerializer.Serialize(NewBinding == null ? null : new { Host = NewBinding.Host.ToString(), Network = NewBinding.Prefix.ToString(), Mask = NewBinding.Mask.Identifier.ToString()   }) },
        };
    }
}
