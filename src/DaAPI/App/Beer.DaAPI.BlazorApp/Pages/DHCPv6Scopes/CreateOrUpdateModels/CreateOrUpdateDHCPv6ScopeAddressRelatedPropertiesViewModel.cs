using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Common.DHCPv6;
using Beer.DaAPI.Shared.Responses;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1;
using static Beer.DaAPI.Shared.Requests.DHCPv6ScopeRequests.V1.DHCPv6ScopeAddressPropertyReqest;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public class ExcludedAddressItem : ValueObjectWithParent<String, CreateOrUpdateDHCPv6ScopeAddressRelatedPropertiesViewModel>
    {
        public ExcludedAddressItem(String value, CreateOrUpdateDHCPv6ScopeAddressRelatedPropertiesViewModel parent) : base(
            value ?? String.Empty, parent)
        {
        }
    }

    public enum RenewTypes
    {
        Static = 0,
        Dynamic = 1,
    }

    public class CreateOrUpdateDHCPv6ScopeAddressRelatedPropertiesViewModel
    {
        public NullableOverrideProperty<Double> T1 { get; set; } = new();
        public NullableOverrideProperty<Double> T2 { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> ValidLifetime { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> PreferredLifetime { get; set; } = new();

        public NullableOverrideProperty<Boolean> RapitCommitEnabled { get; set; } = new();
        public NullableOverrideProperty<Boolean> SupportDirectUnicast { get; set; } = new();
        public NullableOverrideProperty<Boolean> AcceptDecline { get; set; } = new();
        public NullableOverrideProperty<Boolean> InformsAreAllowd { get; set; } = new();
        public NullableOverrideProperty<Boolean> ReuseAddressIfPossible { get; set; } = new();

        public NullableOverrideProperty<RenewTypes> RenewType { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> DynamicRenewTime { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> DynamicRenewDeltaToRebound { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> DynamicRenewDeltaToLifetime { get; set; } = new();

        public NullableOverrideProperty<AddressAllocationStrategies> AddressAllocationStrategy { get; set; } = new();

        public String Start { get; set; }
        public String End { get; set; }
        public IList<ExcludedAddressItem> ExcludedAddresses { get; set; } = new List<ExcludedAddressItem>();

        public Boolean DelegatePrefixes { get; set; }
        public String Prefix { get; set; }
        public Byte PrefixLength { get; set; }
        public Byte AssingedPrefixLength { get; set; }

        public CreateOrUpdateDHCPv6ScopeAddressRelatedPropertiesViewModel ParentValues { get; set; }

        public void SetDefaults()
        {
            Start = "fe80::1";
            End = "fe80:fff:ffff:ffff::1";

            T1 = new() { NullableValue = 0.5 };
            T2 = new() { NullableValue = 0.8 };
            ValidLifetime = new() { NullableValue = TimeSpan.FromHours(24) };
            PreferredLifetime = new() { NullableValue = TimeSpan.FromHours(18) };
            RapitCommitEnabled = new() { NullableValue = true };
            SupportDirectUnicast = new() { NullableValue = true };
            AcceptDecline = new() { NullableValue = false };
            InformsAreAllowd = new() { NullableValue = true };
            ReuseAddressIfPossible = new() { NullableValue = false };
            DelegatePrefixes = false;
            AssingedPrefixLength = 60;
            PrefixLength = 56;

            AddressAllocationStrategy = new() { NullableValue = AddressAllocationStrategies.Random };

            RenewType = new() { NullableValue = RenewTypes.Static };
            DynamicRenewTime = new() { NullableValue = TimeSpan.FromHours(3) };
            DynamicRenewDeltaToRebound = new() { NullableValue = TimeSpan.FromHours(1) };
            DynamicRenewDeltaToLifetime = new() { NullableValue = TimeSpan.FromHours(2) };
        }

        internal void RemoveParent()
        {
            ParentValues = null;
            SetDefaults();
        }

        public void AddDefaultExcludedAddress()
        {
            ExcludedAddresses.Add(new ExcludedAddressItem(Start.AsIPv6Address().ToString(), this));
        }

        internal void RemoveExcludedAddress(Int32 index)
        {
            if (index < ExcludedAddresses.Count)
            {
                ExcludedAddresses.RemoveAt(index);
            }
        }

        internal void SetParent(DHCPv6ScopePropertiesResponse parent)
        {
            ParentValues = new()
            {
                Start = parent.AddressRelated.Start,
                End = parent.AddressRelated.End,
                ExcludedAddresses = parent.AddressRelated.ExcludedAddresses.Select(x => new ExcludedAddressItem(x, this)).ToList(),


                RapitCommitEnabled = new() { NullableValue = parent.AddressRelated.RapitCommitEnabled.Value },
                SupportDirectUnicast = new() { NullableValue = parent.AddressRelated.SupportDirectUnicast.Value },
                AcceptDecline = new() { NullableValue = parent.AddressRelated.AcceptDecline.Value },

                DelegatePrefixes = parent.AddressRelated.PrefixDelegationInfo != null,

                InformsAreAllowd = new() { NullableValue = parent.AddressRelated.InformsAreAllowd.Value },
                ReuseAddressIfPossible = new() { NullableValue = parent.AddressRelated.ReuseAddressIfPossible.Value },
                AddressAllocationStrategy = new() { NullableValue = parent.AddressRelated.AddressAllocationStrategy.Value },
                RenewType = new() { NullableValue = parent.AddressRelated.UseDynamicRenew.Value == true ? RenewTypes.Dynamic : RenewTypes.Static },
            };

            if (parent.AddressRelated.DynamicRenew != null)
            {
                ParentValues.DynamicRenewTime = new() { NullableValue = new TimeSpan(parent.AddressRelated.DynamicRenew.Hours, parent.AddressRelated.DynamicRenew.Minutes, 0) };
                ParentValues.DynamicRenewDeltaToRebound = new() { NullableValue = TimeSpan.FromMinutes(parent.AddressRelated.DynamicRenew.DelayToRebound) };
                ParentValues.DynamicRenewDeltaToLifetime = new() { NullableValue = TimeSpan.FromMinutes(parent.AddressRelated.DynamicRenew.DelayToLifetime) };

                ParentValues.T1 = new() { NullableValue = 0.5 };
                ParentValues.T2 = new() { NullableValue = 0.8 };
                ParentValues.ValidLifetime = new() { NullableValue = TimeSpan.FromHours(24) };
                ParentValues.PreferredLifetime = new() { NullableValue = TimeSpan.FromHours(18) };

            }
            else
            {
                ParentValues.T1 = new() { NullableValue = parent.AddressRelated.T1 };
                ParentValues.T2 = new() { NullableValue = parent.AddressRelated.T2 };
                ParentValues.ValidLifetime = new() { NullableValue = parent.AddressRelated.ValidLifetime };
                ParentValues.PreferredLifetime = new() { NullableValue = parent.AddressRelated.PreferedLifetime };

                ParentValues.DynamicRenewTime = new() { NullableValue = new TimeSpan(3, 0, 0) };
                ParentValues.DynamicRenewDeltaToRebound = new() { NullableValue = TimeSpan.FromMinutes(30) };
                ParentValues.DynamicRenewDeltaToLifetime = new() { NullableValue = TimeSpan.FromMinutes(60) };
            }

            if (ParentValues.DelegatePrefixes == true)
            {
                ParentValues.Prefix = parent.AddressRelated.PrefixDelegationInfo.Prefix;
                ParentValues.PrefixLength = parent.AddressRelated.PrefixDelegationInfo.PrefixLength;
                ParentValues.AssingedPrefixLength = parent.AddressRelated.PrefixDelegationInfo.AssingedPrefixLength;
            }

            Start = ParentValues.Start;
            End = ParentValues.End;
            if (parent.AddressRelated.DynamicRenew != null)
            {
                T1 = new() { NullableValue = 0.5 };
                T2 = new() { NullableValue = 0.8 };
                ValidLifetime = new() { NullableValue = TimeSpan.FromHours(24) };
                PreferredLifetime = new() { NullableValue = TimeSpan.FromHours(18) };
            }   
            else
            {
                T1 = new() { DefaultValue = parent.AddressRelated.T1.Value };
                T2 = new() { DefaultValue = parent.AddressRelated.T2.Value };
                ValidLifetime = new() { DefaultValue = parent.AddressRelated.ValidLifetime.Value };
                PreferredLifetime = new() { DefaultValue = parent.AddressRelated.PreferedLifetime.Value };
            }


            RapitCommitEnabled = new() { DefaultValue = parent.AddressRelated.RapitCommitEnabled.Value };
            SupportDirectUnicast = new() { DefaultValue = parent.AddressRelated.SupportDirectUnicast.Value };
            AcceptDecline = new() { DefaultValue = parent.AddressRelated.AcceptDecline.Value };
            InformsAreAllowd = new() { DefaultValue = parent.AddressRelated.InformsAreAllowd.Value };
            ReuseAddressIfPossible = new() { DefaultValue = parent.AddressRelated.ReuseAddressIfPossible.Value };
            AddressAllocationStrategy = new() { DefaultValue = parent.AddressRelated.AddressAllocationStrategy.Value };

            DelegatePrefixes = false;
            Prefix = ParentValues.Prefix;
            PrefixLength = ParentValues.PrefixLength;
            AssingedPrefixLength = ParentValues.AssingedPrefixLength;

            RenewType = new() { DefaultValue = parent.AddressRelated.UseDynamicRenew.Value == true ? RenewTypes.Dynamic : RenewTypes.Static };
            DynamicRenewTime = new() { DefaultValue = parent.AddressRelated.UseDynamicRenew == true ? new TimeSpan(parent.AddressRelated.DynamicRenew.Hours, parent.AddressRelated.DynamicRenew.Minutes, 0) : new TimeSpan(3, 0, 0) };
            DynamicRenewDeltaToRebound = new() { DefaultValue = parent.AddressRelated.UseDynamicRenew == true ? TimeSpan.FromMinutes(parent.AddressRelated.DynamicRenew.DelayToRebound) : TimeSpan.FromHours(1) };
            DynamicRenewDeltaToLifetime = new() { DefaultValue = parent.AddressRelated.UseDynamicRenew == true ? TimeSpan.FromMinutes(parent.AddressRelated.DynamicRenew.DelayToLifetime) : TimeSpan.FromHours(2) };
        }

        internal DHCPv6ScopeAddressPropertyReqest GetRequest()
        {
            DHCPv6ScopeAddressPropertyReqest request = new()
            {
                Start = Start,
                End = End,
                PrefixDelgationInfo = DelegatePrefixes == false ? null : new DHCPv6PrefixDelgationInfoRequest
                {
                    AssingedPrefixLength = AssingedPrefixLength,
                    Prefix = Prefix,
                    PrefixLength = PrefixLength,
                },
                ExcludedAddresses = ExcludedAddresses.Select(x => x.Value).ToList(),
                ValidLifeTime = ValidLifetime.NullableValue,
                PreferredLifeTime = PreferredLifetime.NullableValue,
                T1 = T1.NullableValue,
                T2 = T2.NullableValue,
                RapitCommitEnabled = RapitCommitEnabled.NullableValue,
                AcceptDecline = AcceptDecline.NullableValue,
                InformsAreAllowd = InformsAreAllowd.NullableValue,
                ReuseAddressIfPossible = ReuseAddressIfPossible.NullableValue,
                SupportDirectUnicast = SupportDirectUnicast.NullableValue,
                AddressAllocationStrategy = AddressAllocationStrategy.NullableValue,
                DynamicRenewTime = ((RenewType.HasValue && RenewType == RenewTypes.Dynamic) || DynamicRenewTime.HasValue || DynamicRenewDeltaToRebound.HasValue || DynamicRenewDeltaToLifetime.HasValue) ? new DHCPv6DynamicRenewTimeRequest
                { 
                    Hours = (DynamicRenewTime?.NullableValue ?? ParentValues.DynamicRenewTime.Value).Hours,
                    Minutes = (DynamicRenewTime?.NullableValue ?? ParentValues.DynamicRenewTime.Value).Minutes,
                    MinutesToRebound = (Int32)(DynamicRenewDeltaToRebound?.NullableValue ?? ParentValues.DynamicRenewDeltaToRebound.Value).TotalMinutes,
                    MinutesToEndOfLife = (Int32)(DynamicRenewDeltaToLifetime?.NullableValue ?? ParentValues.DynamicRenewDeltaToLifetime.Value).TotalMinutes,
                } : null,
            };

            if (RenewType.HasValue == true && RenewType == RenewTypes.Static && ParentValues != null && ParentValues.RenewType == RenewTypes.Dynamic)
            {
                request.T1 ??= ParentValues.T1.NullableValue;
                request.T2 ??= ParentValues.T2.NullableValue;
                request.PreferredLifeTime ??= ParentValues.PreferredLifetime.NullableValue;
                request.ValidLifeTime ??= ParentValues.ValidLifetime.NullableValue;

                request.DynamicRenewTime = null;
            }

            return request;
        }

        internal void SetByResponse(DHCPv6ScopePropertiesResponse properties)
        {
            var addressRelatedProperties = properties.AddressRelated;

            Start = addressRelatedProperties.Start;
            End = addressRelatedProperties.End;

            foreach (var item in properties.AddressRelated.ExcludedAddresses)
            {
                ExcludedAddresses.Add(new ExcludedAddressItem(item, this));
            }

            if (addressRelatedProperties.PrefixDelegationInfo != null)
            {
                DelegatePrefixes = true;
                Prefix = addressRelatedProperties.PrefixDelegationInfo.Prefix;
                PrefixLength = addressRelatedProperties.PrefixDelegationInfo.PrefixLength;
                AssingedPrefixLength = addressRelatedProperties.PrefixDelegationInfo.AssingedPrefixLength;
            }
            else
            {
                DelegatePrefixes = false;
            }

            if (addressRelatedProperties.UseDynamicRenew.HasValue == true)
            {
                if (addressRelatedProperties.UseDynamicRenew.Value == true)
                {
                    RenewType.UpdateNullableValue(RenewTypes.Dynamic, ParentValues != null, (x, y) => x == y);
                    DynamicRenewTime.UpdateNullableValue(new TimeSpan(addressRelatedProperties.DynamicRenew.Hours, addressRelatedProperties.DynamicRenew.Minutes, 0), ParentValues != null);
                    DynamicRenewDeltaToRebound.UpdateNullableValue(TimeSpan.FromMinutes(addressRelatedProperties.DynamicRenew.DelayToRebound), ParentValues != null);
                    DynamicRenewDeltaToLifetime.UpdateNullableValue(TimeSpan.FromMinutes(addressRelatedProperties.DynamicRenew.DelayToLifetime), ParentValues != null);
                }
                else
                {
                    RenewType.UpdateNullableValue(RenewTypes.Static, ParentValues != null, (x, y) => x == y);
                    DynamicRenewTime.UpdateNullableValue(new TimeSpan(3, 0, 0), ParentValues != null);
                    DynamicRenewDeltaToRebound.UpdateNullableValue(TimeSpan.FromHours(1), ParentValues != null);
                    DynamicRenewDeltaToLifetime.UpdateNullableValue(TimeSpan.FromHours(2), ParentValues != null);
                }
            }
            else
            {
                RenewType.UpdateNullableValue(default, ParentValues != null);
                DynamicRenewTime.UpdateNullableValue(default, ParentValues != null);
                DynamicRenewDeltaToRebound.UpdateNullableValue(default, ParentValues != null);
                DynamicRenewDeltaToLifetime.UpdateNullableValue(default, ParentValues != null);
            }

            PreferredLifetime.UpdateNullableValue(addressRelatedProperties.PreferedLifetime, ParentValues != null);
            ValidLifetime.UpdateNullableValue(addressRelatedProperties.ValidLifetime, ParentValues != null);
            T1.UpdateNullableValue(addressRelatedProperties.T1, ParentValues != null);
            T2.UpdateNullableValue(addressRelatedProperties.T2, ParentValues != null);

            RapitCommitEnabled.UpdateNullableValue(addressRelatedProperties.RapitCommitEnabled, ParentValues != null);
            SupportDirectUnicast.UpdateNullableValue(addressRelatedProperties.SupportDirectUnicast, ParentValues != null);
            AcceptDecline.UpdateNullableValue(addressRelatedProperties.AcceptDecline, ParentValues != null);
            InformsAreAllowd.UpdateNullableValue(addressRelatedProperties.InformsAreAllowd, ParentValues != null);
            ReuseAddressIfPossible.UpdateNullableValue(addressRelatedProperties.ReuseAddressIfPossible, ParentValues != null);
            AddressAllocationStrategy.UpdateNullableValue(addressRelatedProperties.AddressAllocationStrategy, ParentValues != null, (v1, v2) => v1 == v2);
        }
    }

    public class CreateOrUpdateDHCPv6ScopeAdressRelatedPropertiesViewModelValidator : AbstractValidator<CreateOrUpdateDHCPv6ScopeAddressRelatedPropertiesViewModel>
    {
        public CreateOrUpdateDHCPv6ScopeAdressRelatedPropertiesViewModelValidator(IStringLocalizer<CreateOrUpdateDHCPv6ScopePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(y => y.Start).Cascade(CascadeMode.Stop).InjectIPv6AdressValidator(localizer["StartAddressLabel"]);
            RuleFor(y => y.Start).Must((properties, startAddress) => startAddress.AsIPv6Address() <= properties.End.AsIPv6Address()).WithMessage(localizer["ValidationStartSmallerThenEnd"]).When(x => x.End.AsIPv6Address() != IPv6Address.Empty, ApplyConditionTo.CurrentValidator).WithName(localizer["StartAddressLabel"]);

            RuleFor(y => y.End).Cascade(CascadeMode.Stop).InjectIPv6AdressValidator(localizer["EndAddressLabel"]);
            RuleFor(y => y.End).Must((properties, endAddress) => endAddress.AsIPv6Address() >= properties.Start.AsIPv6Address()).WithMessage(localizer["ValidationEndGreaterThenStart"]).When(x => x.Start.AsIPv6Address() != IPv6Address.Empty, ApplyConditionTo.CurrentValidator).WithName(localizer["EndAddressLabel"]);

            When(x => x.DelegatePrefixes == true, () =>
            {
                RuleFor(y => y.PrefixLength).LessThanOrEqualTo((Byte)128).Must((y, z) => z <= y.AssingedPrefixLength).WithMessage(localizer["ValidationPrefixLengthGreaterThanAssingedPrefixLength"]).WithName("PrefixDelegationLengthLabel");
                RuleFor(y => y.AssingedPrefixLength).LessThanOrEqualTo((Byte)128).Must((y, z) => z >= y.PrefixLength).WithMessage(localizer["ValidationAssingedPrefixLengthSmallerThanPrefixLength"]).WithName("PrefixDelegationAssingedLengthLabel");
                RuleFor(y => y.Prefix).Cascade(CascadeMode.Stop).InjectIPv6AdressValidator(localizer["PrefixDelegationNetworkLabel"]).Must((x, y) =>
                {
                    try
                    {
                        var address = IPv6Address.FromString(y);
                        IPv6SubnetMask mask = new(new IPv6SubnetMaskIdentifier(x.PrefixLength));
                        return mask.IsIPv6AdressANetworkAddress(address);
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                }).WithMessage(localizer["ValidationNotAValidNetwork"]);
            });

            When(x => x.ParentValues != null, () =>
            {
                RuleFor(y => y.Start).Cascade(CascadeMode.Stop)
                .Must((properties, address) => address.AsIPv6Address() >= properties.ParentValues.Start.AsIPv6Address()).WithMessage(localizer["ValidationStartAddressSmallerThenInParentRange"])
                .Must((properties, address) => address.AsIPv6Address() <= properties.ParentValues.End.AsIPv6Address()).WithMessage(localizer["ValidationStartAddressGreaterThenInParentRangeEnd"]);

                RuleFor(y => y.End).Cascade(CascadeMode.Stop)
                 .Must((properties, address) => address.AsIPv6Address() <= properties.ParentValues.End.AsIPv6Address()).WithMessage(localizer["ValidationEndAddressBiggerThenEndInParentRange"])
                 .Must((properties, address) => address.AsIPv6Address() >= properties.ParentValues.Start.AsIPv6Address()).WithMessage(localizer["ValidationEndAddressSmallerThenStartInParentRange"]);

            }).Otherwise(() =>
            {
                RuleFor(x => x.PreferredLifetime.NullableValue).NotEmpty().WithName(localizer["PreferredLifetimeLabel"]);
                RuleFor(x => x.ValidLifetime.NullableValue).NotEmpty().WithName(localizer["ValidLifetimeTimeLabel"]);

                RuleFor(x => x.T1.NullableValue).NotEmpty();
                RuleFor(x => x.T2.NullableValue).NotEmpty();

                RuleFor(x => x.SupportDirectUnicast.NullableValue).NotEmpty().WithName(localizer["SupportUnicastLabel"]);
                RuleFor(x => x.RapitCommitEnabled.NullableValue).NotEmpty().WithName(localizer["RapitCommitEnabledLabel"]);
                RuleFor(x => x.AcceptDecline.NullableValue).NotEmpty().WithName(localizer["AccpetDeclinesLabel"]);
                RuleFor(x => x.InformsAreAllowd.NullableValue).NotEmpty().WithName(localizer["AccpetInformsLabel"]);
                RuleFor(x => x.ReuseAddressIfPossible.NullableValue).NotEmpty().WithName(localizer["ReuseAddressLabel"]);
                RuleFor(x => x.AddressAllocationStrategy.NullableValue).NotEmpty().WithName(localizer["AddressAllocationStrategyLabel"]);
            });

            RuleForEach(x => x.ExcludedAddresses).ChildRules(element =>
            {
                element.RuleFor(x => x.Value).Cascade(CascadeMode.Stop).InjectIPv6AdressValidator(localizer["SingleExcludedAddressLabel"]);
                element.RuleFor(x => x.Value)
                .Must((properties, address) => address.AsIPv6Address() >= properties.Parent.Start.AsIPv6Address()).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressSmallerThenStart"])
                .Must((properties, address) => address.AsIPv6Address() <= properties.Parent.End.AsIPv6Address()).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressGreaterThenEnd"])
                .Must((properties, address) => properties.Parent.ExcludedAddresses.Count(y => y.Value == address) == 1).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressAlreadyExists"])
                .Must((properties, address) => !properties.Parent.ParentValues.ExcludedAddresses.Any(y => y.Value == address)).WithMessage(localizer["ValidationExcludedAddressAlreadyExists"]).When(x => x.Parent.ParentValues != null, ApplyConditionTo.CurrentValidator);
            });

            When(y => y.PreferredLifetime.NullableValue.HasValue, () =>
            {
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.ValidLifetime.Value).WithMessage(localizer["ValidationPreferredLifetimeGreaterThanValidLifetime"]).When(y => y.ValidLifetime.HasValue);
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.ParentValues.ValidLifetime.Value).WithMessage(localizer["ValidationPreferredLifetimeGreaterThanValidLifetimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.ValidLifetime.HasValue && y.ValidLifetime.HasValue == false);
            });

            When(y => y.ValidLifetime.NullableValue.HasValue, () =>
            {
                Transform(x => x.ValidLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.PreferredLifetime.Value).WithMessage(localizer["ValidationValidLifetimeSmallerThanPreferredLifetime"]).When(y => y.PreferredLifetime.HasValue);
                Transform(x => x.ValidLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.ParentValues.PreferredLifetime.Value).WithMessage(localizer["ValidationLifetimeSmallerThanPreferredLifetimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.PreferredLifetime.HasValue && y.PreferredLifetime.HasValue == false);
            });

            When(y => y.T1.NullableValue.HasValue, () =>
            {
                Transform(x => x.T1.NullableValue, (t) => t.Value).GreaterThanOrEqualTo(0.0).LessThanOrEqualTo(1.0).WithName(localizer["T1Label"]);
                Transform(x => x.T1.NullableValue, (t) => t.Value).Must((properties, t1) => t1 <= properties.T2.Value).WithMessage(localizer["ValidationT1BiggerThanT2"]).When(y => y.T2.HasValue);
                Transform(x => x.T1.NullableValue, (t) => t.Value).Must((properties, t1) => t1 <= properties.ParentValues.T2.Value).WithMessage(localizer["ValidationT1BiggerThanT2OfParent"]).When(y => y.ParentValues != null && y.ParentValues.T2.HasValue && y.T2.HasValue == false);
            });

            When(y => y.T2.NullableValue.HasValue, () =>
            {
                Transform(x => x.T2.NullableValue, (t) => t.Value).GreaterThanOrEqualTo(0.0).LessThanOrEqualTo(1.0).WithName(localizer["T2Label"]);
                Transform(x => x.T2.NullableValue, (t) => t.Value).Must((properties, t2) => t2 >= properties.T1.Value).WithMessage(localizer["ValidationT2SmallerThanT1"]).When(y => y.T1.HasValue);
                Transform(x => x.T2.NullableValue, (t) => t.Value).Must((properties, t2) => t2 >= properties.ParentValues.T1.Value).WithMessage(localizer["ValidationT2SmallerThanT1OfParent"]).When(y => y.ParentValues != null && y.ParentValues.T1.HasValue && y.T1.HasValue == false);
            });

        }
    }
}
