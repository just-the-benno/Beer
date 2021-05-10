using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Pages.Clients
{
    public class CreateClientValidatior : AbstractValidator<CreateClientViewModel>
    {
        public CreateClientValidatior(
            IStringLocalizer<CreateClientPage> localizer)
        {
            RuleFor(user => user.DisplayName).NotEmpty().MinimumLength(3).MaximumLength(50).WithName(localizer["DisplayNameLabel"]);
            RuleFor(user => user.ClientId).NotEmpty().MinimumLength(3).MaximumLength(50).Matches(@"^[a-zA-Z0-9_-]*$").WithMessage(localizer["ValidationClientIdInvalidChars"]).WithName(localizer["ClientIdLabel"]);
            
            RuleFor(user => user.Password).NotEmpty().MinimumLength(3).MaximumLength(50).WithName(localizer["PasswordLabel"]);
            RuleFor(user => user.FrontChannelLogoutUri).Must(x => Uri.TryCreate(x, UriKind.Absolute, out Uri _)).WithMessage(localizer["ValidationInvalidUri"]).When(x => String.IsNullOrEmpty(x.FrontChannelLogoutUri) == false);

            RuleFor(user => user.RedirectUris).Must(x => x.Count() > 0).WithMessage(localizer["ValidationNoRedirectUris"]);
            RuleFor(user => user.PostLogoutRedirectUris).Must(x => x.Count() > 0).WithMessage(localizer["ValidationNoLogoutCallbackUris"]);

            RuleForEach(x => x.RedirectUris).Must(x => Uri.TryCreate(x.Value, UriKind.Absolute, out Uri _)).WithMessage(localizer["ValidationInvalidUri"]);
            RuleForEach(x => x.AllowedCorsOrigins).Must(x => Uri.TryCreate(x.Value, UriKind.Absolute, out Uri _)).WithMessage(localizer["ValidationInvalidUri"]);
            RuleForEach(x => x.PostLogoutRedirectUris).Must(x => Uri.TryCreate(x.Value, UriKind.Absolute, out Uri _)).WithMessage(localizer["ValidationInvalidUri"]);
            TransformForEach(x => x.AllowedScopes, y => y.Value).NotEmpty().MinimumLength(1).MaximumLength(100).Matches(@"^[a-zA-Z0-9_-]*$").WithMessage(localizer["ValidationScopeInvalidChars"]).WithName(localizer["ScopeLabel"]);
        }
    }
}
