using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public class CreateNotificationPipelineGenerellPropertiesViewModel
    {
        public String Name { get; set; }
        public String Description { get; set; }
    }

    public class CreateNotificationPipelineGenerellPropertiesViewModelValidator : AbstractValidator<CreateNotificationPipelineGenerellPropertiesViewModel>
    {
        public CreateNotificationPipelineGenerellPropertiesViewModelValidator(IStringLocalizer<CreateNotificationPipelinePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100).WithName(localizer["PipelineNameLabel"]);
            RuleFor(x => x.Description).MinimumLength(3).When(x => String.IsNullOrEmpty(x.Description) == false).MaximumLength(250).WithName(localizer["PipelineDescriptionLabel"]);
        }
    }

}
