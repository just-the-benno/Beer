using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public class CreateNotificationPipelineTriggerPropertiesViewModel
    {
        public String TriggerName { get; set; }
    }

    public class CreateNotificationPipelineTriggerPropertiesViewModelValidator : AbstractValidator<CreateNotificationPipelineTriggerPropertiesViewModel>
    {
        public CreateNotificationPipelineTriggerPropertiesViewModelValidator(IStringLocalizer<CreateNotificationPipelinePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(x => x.TriggerName).NotEmpty().WithName(localizer["TriggerLabel"]);
        }
    }
}
