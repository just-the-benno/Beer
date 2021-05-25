using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv4ScopeAddressPropertiesConverter : JsonConverter
    {
        private class EeasySerialibleVersionOfDHCPv6ScopeAddressProperties
        {
            public IPv4Address Start { get; set; }
            public IPv4Address End { get; set; }

            public Boolean? ReuseAddressIfPossible { get; set; }

            public Boolean? SupportDirectUnicast { get; set; }
            public Boolean? AcceptDecline { get; set; }
            public Boolean? InformsAreAllowd { get; set; }

            public TimeSpan? LeaseTime { get; set; }
            public TimeSpan? RenewalTime { get; set; }
            public TimeSpan? PreferredLifetime { get; set; }

            public Byte? NetworkMask { get; set; }

            public Boolean? UseDynamicAddress { get; set; }
            public Int32? DynamicAddressHour { get; set; }
            public Int32? DynamicAddressMinute { get; set; }
            public Int32? DynamicAddressReboundingInterval { get; set; }
            public Int32? DynamicAddressLeaseInterval { get; set; }

            public IEnumerable<IPv4Address> ExcludedAddresses { get; set; }
            public DHCPv4ScopeAddressProperties.AddressAllocationStrategies? AddressAllocationStrategy { get; set; }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv4ScopeAddressProperties);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<EeasySerialibleVersionOfDHCPv6ScopeAddressProperties>(reader);

            DHCPv4ScopeAddressProperties result = null;
            if(info.UseDynamicAddress == true)
            {
                DynamicRenewTime time = DynamicRenewTime.WithSpecificRange(
                    info.DynamicAddressHour.Value, info.DynamicAddressMinute.Value,
                    info.DynamicAddressReboundingInterval.Value, info.DynamicAddressLeaseInterval.Value);

                result = new DHCPv4ScopeAddressProperties(info.Start, info.End, info.ExcludedAddresses,
                   time, info.NetworkMask,
                   reuseAddressIfPossible: info.ReuseAddressIfPossible, addressAllocationStrategy: info.AddressAllocationStrategy,
                   supportDirectUnicast: info.SupportDirectUnicast, acceptDecline: info.AcceptDecline, informsAreAllowd: info.InformsAreAllowd
                   );
            }
            else
            {
                result = new DHCPv4ScopeAddressProperties(info.Start, info.End, info.ExcludedAddresses,
                   renewalTime: info.RenewalTime, preferredLifetime: info.PreferredLifetime, leaseTime: info.LeaseTime, info.NetworkMask,
                   reuseAddressIfPossible: info.ReuseAddressIfPossible, addressAllocationStrategy: info.AddressAllocationStrategy,
                   supportDirectUnicast: info.SupportDirectUnicast, acceptDecline: info.AcceptDecline, informsAreAllowd: info.InformsAreAllowd
                   );
            }


            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DHCPv4ScopeAddressProperties item = (DHCPv4ScopeAddressProperties)value;

            var model = new EeasySerialibleVersionOfDHCPv6ScopeAddressProperties
            {
                AcceptDecline = item.AcceptDecline,
                AddressAllocationStrategy = item.AddressAllocationStrategy,
                End = item.End,
                InformsAreAllowd = item.InformsAreAllowd,
                ReuseAddressIfPossible = item.ReuseAddressIfPossible,
                Start = item.Start,
                SupportDirectUnicast = item.SupportDirectUnicast,
                PreferredLifetime = item.PreferredLifetime,
                LeaseTime = item.LeaseTime,
                RenewalTime = item.RenewalTime,
                ExcludedAddresses = item.ExcludedAddresses,
                NetworkMask = item.Mask == null ? new Byte?() : (Byte)item.Mask.GetSlashNotation(),
                UseDynamicAddress = item.UseDynamicRewnewTime
            };

            if(model.UseDynamicAddress == true)
            {
                model.DynamicAddressHour = item.DynamicRenewTime.Hour;
                model.DynamicAddressMinute = item.DynamicRenewTime.Minutes;
                model.DynamicAddressReboundingInterval = (Int32)item.DynamicRenewTime.MinutesToRebound;
                model.DynamicAddressLeaseInterval = (Int32)item.DynamicRenewTime.MinutesToEndOfLife;
            }

            serializer.Serialize(writer, model);
        }
    }
}
