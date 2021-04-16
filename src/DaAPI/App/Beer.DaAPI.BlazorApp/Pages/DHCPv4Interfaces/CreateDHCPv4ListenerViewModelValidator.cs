using Beer.DaAPI.BlazorApp.Helper;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Interfaces
{
    public class CreateDHCPv4ListenerViewModelValidator :  AbstractValidator<CreateDHCPv4ListenerViewModel>
    {
        public CreateDHCPv4ListenerViewModelValidator(IStringLocalizer<CreateDHCPv4InterfaceDialog> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(user => user.Name).NotEmpty().MinimumLength(3).MaximumLength(100).WithName(localizer["NameLabel"]);
            RuleFor(user => user.IPv4Address).InjectIPv4AdressValidator(localizer["IPAddressLabel"]).WithName(localizer["IPAddressLabel"]);
            RuleFor(user => user.InterfaceId).NotEmpty().WithName(localizer["InterfaceIdLabel"]);
        }
    }
}
