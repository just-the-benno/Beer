using Beer.DaAPI.BlazorApp.Helper;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Interfaces
{
    public class CreateDHCPv4ListenerViewModelValidator : AbstractValidator<CreateDHCPv6ListenerViewModel>
    {
        public CreateDHCPv4ListenerViewModelValidator(IStringLocalizer<CreateDHCPv6InterfaceDialog> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(user => user.Name).NotEmpty().MinimumLength(3).MaximumLength(100).WithName(localizer["NameLabel"]);
            RuleFor(user => user.IPv6Address).NotNull().InjectIPv6AdressValidator(localizer["IPAddressLabel"]).WithName(localizer["IPAddressLabel"]);
            RuleFor(user => user.InterfaceId).NotEmpty().WithName(localizer["InterfaceIdLabel"]);
        }
    }
}
