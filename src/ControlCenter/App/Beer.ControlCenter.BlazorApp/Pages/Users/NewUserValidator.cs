using Beer.ControlCenter.BlazorApp.Services;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Pages.Users
{
    public class NewUserValidator : AbstractValidator<CreateNewUserViewModel>
    {
        public NewUserValidator(
            IBeerUserService userService,
            IStringLocalizer<NewUserPage> nameLocalizer,
            IStringLocalizer<UserPasswordValidationResource> localizer)
        {
            RuleFor(user => user.Loginname).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(3).MaximumLength(50).MustAsync(async(name, cancellation) => await userService.CheckIfUsernameExists(name) == false ) 
            .WithMessage(nameLocalizer["LoginNameNotUnique"]).WithName(nameLocalizer["UsernameLabel"]);
            
            RuleFor(user => user.DisplayName).NotEmpty().MinimumLength(3).MaximumLength(50).WithName(nameLocalizer["DisplayNameLabel"]);
            
            RuleFor(user => user.Password).NotEmpty().MinimumLength(6).MaximumLength(50)
                .Matches(@"[a-z]").WithMessage(localizer["NoLowerCaseLetter"])
                .Matches(@"[A-Z]").WithMessage(localizer["NoUpperCaseLetter"])
                .Matches(@"[0-9]").WithMessage(localizer["NoDigit"])
                .Matches(@"[^a-zA-Z\d\s]").WithMessage(localizer["NoNonAlphaNumericCharacter"]).WithName(nameLocalizer["PasswordLabel"]);

            RuleFor(user => user.PasswordConfirmation).NotEmpty()
                .Equal(c => c.Password).WithMessage(localizer["PasswordDoesntMatch"])
                .WithName(nameLocalizer["PasswordConfirmationLabel"]);

            RuleFor(user => user.AvatarUrl).NotEmpty().WithName(nameLocalizer["Avatar"]);
        }
    }
}
