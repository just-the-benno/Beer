﻿using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Infrastructure.StorageEngine.Converters
{
    public class DHCPv6ScopeAddressPropertiesConverter : JsonConverter
    {
        private class EeasySerialibleVersionOfDHCPv6ScopeAddressProperties
        {
            public IPv6Address Start { get; set; }
            public IPv6Address End { get; set; }

            public Boolean? ReuseAddressIfPossible { get; set; }

            public Boolean? SupportDirectUnicast { get; set; }
            public Boolean? AcceptDecline { get; set; }
            public Boolean? InformsAreAllowd { get; set; }

            public Double? T1 { get; set; }
            public Double? T2 { get; set; }

            public TimeSpan? PreferredLeaseTime { get; set; }
            public TimeSpan? ValidLeaseTime { get; set; }
            public Boolean? RapitCommitEnabled { get; set; }

            public Boolean? UseDynamicAddress { get; set; }
            public Int32? DynamicAddressHour { get; set; }
            public Int32? DynamicAddressMinute { get; set; }
            public Int32? DynamicAddressReboundingInterval { get; set; }
            public Int32? DynamicAddressLeaseInterval { get; set; }

            public IEnumerable<IPv6Address> ExcludedAddresses { get; set; }
            public DHCPv6ScopeAddressProperties.AddressAllocationStrategies? AddressAllocationStrategy { get; set; }

            public DHCPv6PrefixDelgationInfo PrefixDelgationInfo { get; set; }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(DHCPv6ScopeAddressProperties);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var info = serializer.Deserialize<EeasySerialibleVersionOfDHCPv6ScopeAddressProperties>(reader);
            DHCPv6ScopeAddressProperties result = null;
            if (info.UseDynamicAddress == true)
            {
                DynamicRenewTime time = DynamicRenewTime.WithSpecificRange(
                   info.DynamicAddressHour.Value, info.DynamicAddressMinute.Value,
                   info.DynamicAddressReboundingInterval.Value, info.DynamicAddressLeaseInterval.Value);

                result = new DHCPv6ScopeAddressProperties(info.Start, info.End, info.ExcludedAddresses, time,
                    reuseAddressIfPossible: info.ReuseAddressIfPossible,addressAllocationStrategy: info.AddressAllocationStrategy,
                    supportDirectUnicast: info.SupportDirectUnicast, acceptDecline: info.AcceptDecline,informsAreAllowd: info.InformsAreAllowd,
                    rapitCommitEnabled: info.RapitCommitEnabled,info.PrefixDelgationInfo);
            }
            else
            {
                result = new DHCPv6ScopeAddressProperties(info.Start, info.End, info.ExcludedAddresses,
                    t1: info.T1.HasValue == true ? DHCPv6TimeScale.FromDouble(info.T1.Value) : null,
                    t2: info.T2.HasValue == true ? DHCPv6TimeScale.FromDouble(info.T2.Value) : null,
                    preferredLifeTime: info.PreferredLeaseTime, validLifeTime: info.ValidLeaseTime, reuseAddressIfPossible: info.ReuseAddressIfPossible, addressAllocationStrategy: info.AddressAllocationStrategy,
                    supportDirectUnicast: info.SupportDirectUnicast, acceptDecline: info.AcceptDecline, informsAreAllowd: info.InformsAreAllowd,
                    rapitCommitEnabled: info.RapitCommitEnabled, info.PrefixDelgationInfo);
            }


            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DHCPv6ScopeAddressProperties item = (DHCPv6ScopeAddressProperties)value;

            var model = new EeasySerialibleVersionOfDHCPv6ScopeAddressProperties
            {
                AcceptDecline = item.AcceptDecline,
                AddressAllocationStrategy = item.AddressAllocationStrategy,
                End = item.End,
                InformsAreAllowd = item.InformsAreAllowd,
                RapitCommitEnabled = item.RapitCommitEnabled,
                ReuseAddressIfPossible = item.ReuseAddressIfPossible,
                Start = item.Start,
                SupportDirectUnicast = item.SupportDirectUnicast,
                PreferredLeaseTime = item.PreferredLeaseTime,
                ValidLeaseTime = item.ValidLeaseTime,
                ExcludedAddresses = item.ExcludedAddresses,
                PrefixDelgationInfo = item.PrefixDelgationInfo,
                T1 = item.T1?.Value,
                T2 = item.T2?.Value,
                UseDynamicAddress = item.UseDynamicRewnewTime
            };

            if (model.UseDynamicAddress == true)
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
