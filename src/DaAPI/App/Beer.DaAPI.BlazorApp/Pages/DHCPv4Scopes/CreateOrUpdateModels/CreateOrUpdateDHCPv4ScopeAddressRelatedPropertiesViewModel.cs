using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Shared.Responses;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1;
using static Beer.DaAPI.Shared.Requests.DHCPv4ScopeRequests.V1.DHCPv4ScopeAddressPropertyReqest;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class ExcludedAddressItem : ValueObjectWithParent<String, CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel>
    {
        public ExcludedAddressItem(String value, CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel parent) : base(
            value ?? String.Empty, parent)
        {
        }
    }

    public class CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel
    {
        public NullableOverrideProperty<Byte> SubnetMask { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> RenewalTime { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> PreferredLifetime { get; set; } = new();
        public NullableOverrideProperty<TimeSpan> LeaseTime { get; set; } = new();

        public NullableOverrideProperty<Boolean> SupportDirectUnicast { get; set; } = new();
        public NullableOverrideProperty<Boolean> AcceptDecline { get; set; } = new();
        public NullableOverrideProperty<Boolean> InformsAreAllowd { get; set; } = new();
        public NullableOverrideProperty<Boolean> ReuseAddressIfPossible { get; set; } = new();

        public NullableOverrideProperty<AddressAllocationStrategies> AddressAllocationStrategy { get; set; } = new();

        public String Start { get; set; }
        public String End { get; set; }
        public IList<ExcludedAddressItem> ExcludedAddresses { get; set; } = new List<ExcludedAddressItem>();

        public CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel ParentValues { get; set; }

        public void SetDefaults()
        {
            Start = "172.16.0.1";
            End = "172.16.255.255";

            SubnetMask = new() { NullableValue = 16 };
            RenewalTime = new() { NullableValue = TimeSpan.FromHours(6) };
            PreferredLifetime = new() { NullableValue = TimeSpan.FromHours(12) };
            LeaseTime = new() { NullableValue = TimeSpan.FromHours(24) };
            SupportDirectUnicast = new() { NullableValue = true };
            AcceptDecline = new() { NullableValue = false };
            InformsAreAllowd = new() { NullableValue = true };
            ReuseAddressIfPossible = new() { NullableValue = false };

            AddressAllocationStrategy = new() { NullableValue = AddressAllocationStrategies.Random };
        }

        internal void RemoveParent()
        {
            ParentValues = null;
            SetDefaults();
        }

        public void AddDefaultExcludedAddress()
        {
            ExcludedAddresses.Add(new ExcludedAddressItem(Start.AsIPv4Address().ToString(), this));
        }

        internal void RemoveExcludedAddress(Int32 index)
        {
            if (index < ExcludedAddresses.Count)
            {
                ExcludedAddresses.RemoveAt(index);
            }
        }

        internal void SetParent(DHCPv4ScopePropertiesResponse parent)
        {
            ParentValues = new()
            {
                Start = parent.AddressRelated.Start,
                End = parent.AddressRelated.End,
                ExcludedAddresses = parent.AddressRelated.ExcludedAddresses.Select(x => new ExcludedAddressItem(x, this)).ToList(),
                SubnetMask = new() { NullableValue = parent.AddressRelated.Mask },
                RenewalTime = new() { NullableValue = parent.AddressRelated.RenewalTime },
                PreferredLifetime = new() { NullableValue = parent.AddressRelated.PreferredLifetime },
                LeaseTime = new() { NullableValue = parent.AddressRelated.LeaseTime },
                SupportDirectUnicast = new() { NullableValue = parent.AddressRelated.SupportDirectUnicast.Value },
                AcceptDecline = new() { NullableValue = parent.AddressRelated.AcceptDecline.Value },
                InformsAreAllowd = new() { NullableValue = parent.AddressRelated.InformsAreAllowd.Value },
                ReuseAddressIfPossible = new() { NullableValue = parent.AddressRelated.ReuseAddressIfPossible.Value },
                AddressAllocationStrategy = new() { NullableValue = parent.AddressRelated.AddressAllocationStrategy.Value },
            };

            Start = ParentValues.Start;
            End = ParentValues.End;

            SubnetMask = new() { DefaultValue = parent.AddressRelated.Mask.Value };
            PreferredLifetime = new() { DefaultValue = parent.AddressRelated.PreferredLifetime.Value };
            RenewalTime = new() { DefaultValue = parent.AddressRelated.RenewalTime.Value };
            LeaseTime = new() { DefaultValue = parent.AddressRelated.LeaseTime.Value };
            SupportDirectUnicast = new() { DefaultValue = parent.AddressRelated.SupportDirectUnicast.Value };
            AcceptDecline = new() { DefaultValue = parent.AddressRelated.AcceptDecline.Value };
            InformsAreAllowd = new() { DefaultValue = parent.AddressRelated.InformsAreAllowd.Value };
            ReuseAddressIfPossible = new() { DefaultValue = parent.AddressRelated.ReuseAddressIfPossible.Value };
            AddressAllocationStrategy = new() { DefaultValue = parent.AddressRelated.AddressAllocationStrategy.Value };

        }

        internal DHCPv4ScopeAddressPropertyReqest GetRequest() => new()
        {
            Start = Start,
            End = End,
            ExcludedAddresses = ExcludedAddresses.Select(x => x.Value).ToList(),
            MaskLength = SubnetMask.NullableValue,
            PreferredLifetime = PreferredLifetime.NullableValue,
            LeaseTime = LeaseTime.NullableValue,
            RenewalTime = RenewalTime.NullableValue,
            AcceptDecline = AcceptDecline.NullableValue,
            InformsAreAllowd = InformsAreAllowd.NullableValue,
            ReuseAddressIfPossible = ReuseAddressIfPossible.NullableValue,
            SupportDirectUnicast = SupportDirectUnicast.NullableValue,
            AddressAllocationStrategy = AddressAllocationStrategy.NullableValue,
        };

        internal void SetByResponse(DHCPv4ScopePropertiesResponse properties)
        {
            var addressRelatedProperties = properties.AddressRelated;

            Start = addressRelatedProperties.Start;
            End = addressRelatedProperties.End;

            SubnetMask.UpdateNullableValue(addressRelatedProperties.Mask, ParentValues != null);

            PreferredLifetime.UpdateNullableValue(addressRelatedProperties.PreferredLifetime, ParentValues != null);
            RenewalTime.UpdateNullableValue(addressRelatedProperties.RenewalTime, ParentValues != null);
            LeaseTime.UpdateNullableValue(addressRelatedProperties.LeaseTime, ParentValues != null);
            SupportDirectUnicast.UpdateNullableValue(addressRelatedProperties.SupportDirectUnicast, ParentValues != null);
            AcceptDecline.UpdateNullableValue(addressRelatedProperties.AcceptDecline, ParentValues != null);
            InformsAreAllowd.UpdateNullableValue(addressRelatedProperties.InformsAreAllowd, ParentValues != null);
            ReuseAddressIfPossible.UpdateNullableValue(addressRelatedProperties.ReuseAddressIfPossible, ParentValues != null);
            AddressAllocationStrategy.UpdateNullableValue(addressRelatedProperties.AddressAllocationStrategy, ParentValues != null, (v1, v2) => v1 == v2);

            foreach (var item in properties.AddressRelated.ExcludedAddresses)
            {
                ExcludedAddresses.Add(new ExcludedAddressItem(item, this));
            }
        }
    }

    public class CreateOrUpdateDHCPv4ScopeAdressRelatedPropertiesViewModelValidator : AbstractValidator<CreateOrUpdateDHCPv4ScopeAddressRelatedPropertiesViewModel>
    {
        public CreateOrUpdateDHCPv4ScopeAdressRelatedPropertiesViewModelValidator(IStringLocalizer<CreateOrUpdateDHCPv4ScopePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(y => y.Start).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["StartAddressLabel"]);
            RuleFor(y => y.Start).Must((properties, startAddress) => startAddress.AsIPv4Address() <= properties.End.AsIPv4Address()).WithMessage(localizer["ValidationStartSmallerThenEnd"]).When(x => x.End.AsIPv4Address() != IPv4Address.Empty, ApplyConditionTo.CurrentValidator).WithName(localizer["StartAddressLabel"]);

            RuleFor(y => y.End).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["EndAddressLabel"]);
            RuleFor(y => y.End).Must((properties, endAddress) => endAddress.AsIPv4Address() >= properties.Start.AsIPv4Address()).WithMessage(localizer["ValidationEndGreaterThenStart"]).When(x => x.Start.AsIPv4Address() != IPv4Address.Empty, ApplyConditionTo.CurrentValidator).WithName(localizer["EndAddressLabel"]);

            When(x => x.ParentValues != null, () =>
            {
                RuleFor(y => y.Start).Cascade(CascadeMode.Stop)
                .Must((properties, address) => address.AsIPv4Address() >= properties.ParentValues.Start.AsIPv4Address()).WithMessage(localizer["ValidationStartAddressSmallerThenInParentRange"])
                .Must((properties, address) => address.AsIPv4Address() <= properties.ParentValues.End.AsIPv4Address()).WithMessage(localizer["ValidationStartAddressGreaterThenInParentRangeEnd"]);

                RuleFor(y => y.End).Cascade(CascadeMode.Stop)
                 .Must((properties, address) => address.AsIPv4Address() <= properties.ParentValues.End.AsIPv4Address()).WithMessage(localizer["ValidationEndAddressBiggerThenEndInParentRange"])
                 .Must((properties, address) => address.AsIPv4Address() >= properties.ParentValues.Start.AsIPv4Address()).WithMessage(localizer["ValidationEndAddressSmallerThenStartInParentRange"]);

            }).Otherwise(() =>
            {
                RuleFor(x => x.SubnetMask.NullableValue).NotEmpty().WithName(localizer["SubnetMaskLabel"]);
                RuleFor(x => x.RenewalTime.NullableValue).NotEmpty().WithName(localizer["RenewalTimeLabel"]);
                RuleFor(x => x.PreferredLifetime.NullableValue).NotEmpty().WithName(localizer["PreferredLifetimeLabel"]);
                RuleFor(x => x.LeaseTime.NullableValue).NotEmpty().WithName(localizer["LeaseTimeLabel"]);
                RuleFor(x => x.SupportDirectUnicast.NullableValue).NotEmpty().WithName(localizer["SupportUnicastLabel"]);
                RuleFor(x => x.AcceptDecline.NullableValue).NotEmpty().WithName(localizer["AccpetDeclinesLabel"]);
                RuleFor(x => x.InformsAreAllowd.NullableValue).NotEmpty().WithName(localizer["AccpetInformsLabel"]);
                RuleFor(x => x.ReuseAddressIfPossible.NullableValue).NotEmpty().WithName(localizer["ReuseAddressLabel"]);
                RuleFor(x => x.AddressAllocationStrategy.NullableValue).NotEmpty().WithName(localizer["AddressAllocationStrategyLabel"]);
            });

            RuleForEach(x => x.ExcludedAddresses).ChildRules(element =>
            {
                element.RuleFor(x => x.Value).Cascade(CascadeMode.Stop).InjectIPv4AdressValidator(localizer["SingleExcludedAddressLabel"]);
                element.RuleFor(x => x.Value)
                .Must((properties, address) => address.AsIPv4Address() >= properties.Parent.Start.AsIPv4Address()).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressSmallerThenStart"])
                .Must((properties, address) => address.AsIPv4Address() <= properties.Parent.End.AsIPv4Address()).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressGreaterThenEnd"])
                .Must((properties, address) => properties.Parent.ExcludedAddresses.Count(y => y.Value == address) == 1).When(x => x.Parent != null, ApplyConditionTo.CurrentValidator).WithMessage(localizer["ValidationExcludedAddressAlreadyExists"])
                .Must((properties, address) => !properties.Parent.ParentValues.ExcludedAddresses.Any(y => y.Value == address)).WithMessage(localizer["ValidationExcludedAddressAlreadyExists"]).When(x => x.Parent.ParentValues != null, ApplyConditionTo.CurrentValidator);
            });

            Transform(x => x.SubnetMask.NullableValue, input => (Int32)input.Value).GreaterThanOrEqualTo(0).LessThanOrEqualTo(32).When(x => x.SubnetMask.HasValue == true).WithName(localizer["SubnetMaskLabel"]);

            When(y => y.RenewalTime.NullableValue.HasValue, () =>
            {
                Transform(x => x.RenewalTime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.PreferredLifetime.Value).WithMessage(localizer["ValidationRenewalTimeGreaterThanPreferredLifetime"]).When(y => y.PreferredLifetime.HasValue);
                Transform(x => x.RenewalTime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.ParentValues.PreferredLifetime.Value).WithMessage(localizer["ValidationRenewalTimeGreaterThanPreferredLifetimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.PreferredLifetime.HasValue && y.PreferredLifetime.HasValue == false);
            });

            When(y => y.PreferredLifetime.NullableValue.HasValue, () =>
            {
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.RenewalTime.Value).WithMessage(localizer["ValidationPreferredLifetimeSmallerRenewalTime"]).When(y => y.RenewalTime.HasValue);
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.ParentValues.RenewalTime.Value).WithMessage(localizer["ValidationPreferredLifetimeSmallerRenewalTimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.RenewalTime.HasValue && y.RenewalTime.HasValue == false);

                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.LeaseTime.Value).WithMessage(localizer["ValidationPreferredLifetimeGreaterThanLeaseTime"]).When(y => y.LeaseTime.HasValue);
                Transform(x => x.PreferredLifetime.NullableValue, (time) => time.Value).Must((properties, time) => time <= properties.ParentValues.LeaseTime.Value).WithMessage(localizer["ValidationPreferredLifetimeGreaterThanLeaseTimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.LeaseTime.HasValue && y.LeaseTime.HasValue == false);
            });

            When(y => y.LeaseTime.NullableValue.HasValue, () =>
            {
                Transform(x => x.LeaseTime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.PreferredLifetime.Value).WithMessage(localizer["ValidationLeaseTimeSmallerThanPreferredLifetime"]).When(y => y.PreferredLifetime.HasValue);
                Transform(x => x.LeaseTime.NullableValue, (time) => time.Value).Must((properties, time) => time >= properties.ParentValues.PreferredLifetime.Value).WithMessage(localizer["ValidationLeaseTimeSmallerThanPreferredLifetimeOfParent"]).When(y => y.ParentValues != null && y.ParentValues.PreferredLifetime.HasValue && y.PreferredLifetime.HasValue == false);
            });

        }
    }
}
