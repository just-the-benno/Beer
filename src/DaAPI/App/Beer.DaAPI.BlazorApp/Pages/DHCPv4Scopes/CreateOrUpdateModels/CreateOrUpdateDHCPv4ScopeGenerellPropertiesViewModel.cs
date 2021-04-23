using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public Boolean HasParent { get; set; }
        public Guid? ParentId { get; set; }
        public Guid Id { get; set; }
    }

    public class CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModelValidator : AbstractValidator<CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModel>
    {
        public CreateOrUpdateDHCPv4ScopeGenerellPropertiesViewModelValidator(IStringLocalizer<CreateOrUpdateDHCPv4ScopePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100).WithName(localizer["NameLabel"]);
            RuleFor(x => x.Description).MinimumLength(3).When(x => String.IsNullOrEmpty(x.Description) == false).MaximumLength(250).WithName(localizer["DescriptionLabel"]);
            RuleFor(x => x.ParentId).NotNull().WithName(localizer["ParentIdLabel"]).When(x => x.HasParent == true);
        }
    }

}
