using Beer.DaAPI.Shared.Responses;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.NotificationPipelineResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public class CreateNotificationPipelineConditionPropertiesViewModel
    {
        private NotificationPipelineDescriptions _descriptions;

        public void SetDescription(NotificationPipelineDescriptions descriptions) => _descriptions = descriptions;

        public IList<NotificationPipelineConditionPropertyEntryViewModel> Entries { get; set; }

        private String _conditionName;

        public String ConditionName
        {
            get => _conditionName;
            set
            {
                _conditionName = value;
                if (String.IsNullOrEmpty(value) == false)
                {
                    Entries = _descriptions.Conditions.First(x => x.Name == value).Properties
                          .Select(x => new NotificationPipelineConditionPropertyEntryViewModel(x.Key, x.Value))
                          .ToList();
                }
                else
                {
                    Entries.Clear();
                }
            }
        }

        internal void SetScopes(IEnumerable<DHCPv6ScopeResponses.V1.DHCPv6ScopeItem> scopes)
        {
            foreach (var item in Entries.Where(x => x.Type == NotificationCondititonDescription.ConditionsPropertyTypes.DHCPv6ScopeList))
            {
                item.SetScopes(scopes);
            }
        }
    }

    public class CreateNotificationPipelineConditionPropertiesViewModelValidator : AbstractValidator<CreateNotificationPipelineConditionPropertiesViewModel>
    {
        public CreateNotificationPipelineConditionPropertiesViewModelValidator(IStringLocalizer<CreateNotificationPipelinePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(x => x.ConditionName).NotEmpty().WithName(localizer["ConditionLabel"]);

            RuleForEach(x => x.Entries).ChildRules(element =>
            {
                element.RuleFor(x => x.Values).Cascade(CascadeMode.Stop).NotNull().NotEmpty().Must(x => x.Any(y => y.IsSelected == true)).WithMessage(localizer["ValidationAtLeastOneScopeSelected"]).When(x => x.Type == NotificationCondititonDescription.ConditionsPropertyTypes.DHCPv6ScopeList);
            }
            );
        }
    }
}
