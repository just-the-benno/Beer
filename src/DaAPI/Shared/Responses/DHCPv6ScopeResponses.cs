using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using System;
using System.Collections.Generic;
using System.Text;
using static Beer.DaAPI.Core.Scopes.ScopeResolverPropertyDescription;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1.DHCPv6ScopeAddressPropertyReqest;

namespace Beer.DaAPI.Shared.Responses
{
    public static class DHCPv6ScopeResponses
    {
        public static class V1
        {
            public class DHCPv6PrefixDelgationInfoResponse
            {
                public String Prefix { get; set; }
                public Byte PrefixLength { get; set; }
                public Byte AssingedPrefixLength { get; set; }
            }

            public abstract class DHCPv6ScopePropertyResponse
            {
                public UInt16 OptionCode { get; set; }
                public DHCPv6ScopePropertyType Type { get; set; }
            }

            public class DHCPv6AddressListScopePropertyResponse : DHCPv6ScopePropertyResponse
            {
                public IEnumerable<String> Addresses { get; set; }
            }

            public class DHCPv6NumericScopePropertyResponse : DHCPv6ScopePropertyResponse
            {
                public Int64 Value { get; set; }
                public NumericScopePropertiesValueTypes NumericType { get; set; }
            }

            public class DHCPv6TextScopePropertyResponse : DHCPv6ScopePropertyResponse
            {
                public String Value { get; set; }
            }

            public class DHCPv6ScopePropertiesResponse
            {
                public String Name { get; set; }
                public String Description { get; set; }
                public Guid? ParentId { get; set; }
                public ScopeResolverResponse Resolver { get; set; }
                public DHCPv6ScopeAddressPropertiesResponse AddressRelated { get; set; }
                public IEnumerable<DHCPv6ScopePropertyResponse> Properties { get; set; }
                public IEnumerable<Int32> InheritanceStopedProperties { get; set; }
            }

            public class DynamicRenewTimeReponse
            {
                public Int32 Hours { get; set; }
                public Int32 Minutes { get; set; }
                public Int32 DelayToRebound { get; set; }
                public Int32 DelayToLifetime { get; set; }
            }

            public class DHCPv6ScopeAddressPropertiesResponse
            {
                public String Start { get; set; }
                public String End { get; set; }
                public IEnumerable<String> ExcludedAddresses { get; set; }
                public Boolean? ReuseAddressIfPossible { get; set; }
                public AddressAllocationStrategies? AddressAllocationStrategy { get; set; }
                public Boolean? UseDynamicRenew { get; set; }
                public DynamicRenewTimeReponse DynamicRenew { get; set; }

                public Double? T1 { get; set; }
                public Double? T2 { get; set; }

                public TimeSpan? PreferedLifetime { get; set; }
                public TimeSpan? ValidLifetime { get; set; }

                public Boolean? SupportDirectUnicast { get; set; }
                public Boolean? AcceptDecline { get; set; }
                public Boolean? InformsAreAllowd { get; set; }
                public Boolean? RapitCommitEnabled { get; set; }
                public DHCPv6PrefixDelgationInfoResponse PrefixDelegationInfo { get; set; }
            }

            public class DHCPv6ScopeResolverDescription
            {
                public String TypeName { get; set; }
                public IEnumerable<DHCPv6ScopeResolverPropertyDescription> Properties { get; set; }
            }

            public class DHCPv6ScopeResolverPropertyDescription
            {
                public String PropertyName { get; set; }
                public ScopeResolverPropertyValueTypes PropertyValueType { get; set; }
            }

            public class DHCPv6ScopeItem
            {
                public String Name { get; set; }
                public Guid Id { get; set; }
                public String StartAddress { get; set; }
                public String EndAddress { get; set; }
            }

            public class DHCPv6ScopeTreeViewItem : DHCPv6ScopeItem
            {
                public IEnumerable<DHCPv6ScopeTreeViewItem> ChildScopes { get; set; }
            }

            public class ScopeResolverResponse
            {
                public String Typename { get; set; }
                public IDictionary<String, String> PropertiesAndValues { get; set; }
            }
        }
    }
}
