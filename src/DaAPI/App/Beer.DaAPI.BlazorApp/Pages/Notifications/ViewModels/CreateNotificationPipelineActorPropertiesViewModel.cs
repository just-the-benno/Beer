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
    public class CreateNotificationPipelineActorPropertiesViewModel
    {
        private NotificationPipelineDescriptions _descriptions;
        public void SetDescription(NotificationPipelineDescriptions descriptions) => _descriptions = descriptions;

        private String _actorName;

        public String ActorName
        {
            get => _actorName;
            set
            {
                _actorName = value;

                if(String.IsNullOrEmpty(value) == false)
                { 
                Entries = _descriptions.Actors.First(x => x.Name == value).Properties
                         .Select(x => new NotificationPipelineActorPropertyEntryViewModel(x.Key, x.Value))
                         .ToList();
                }
                else
                {
                    Entries.Clear();
                }
            }
        }

        public IList<NotificationPipelineActorPropertyEntryViewModel> Entries { get; set; }

        internal void SetScopes(IEnumerable<DHCPv6ScopeResponses.V1.DHCPv6ScopeItem> scopes)
        {
            foreach (var item in Entries.Where(x => x.Type == NotifcationActorDescription.ActorPropertyTypes.DHCPv6ScopeList))
            {
                item.SetScopes(scopes);
            }
        }

    }

    public class NotificationPipelineActorPropertyEntryViewModelValidator : AbstractValidator<CreateNotificationPipelineActorPropertiesViewModel>
    {
        public NotificationPipelineActorPropertyEntryViewModelValidator(IStringLocalizer<CreateNotificationPipelinePage> localizer)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(x => x.ActorName).NotEmpty().WithName(localizer["ActorLabel"]);

            RuleForEach(x => x.Entries).ChildRules(element =>
            {
                element.RuleFor(x => x.Value).NotEmpty().When(x => 
                x.Type == NotifcationActorDescription.ActorPropertyTypes.Endpoint ||
                x.Type == NotifcationActorDescription.ActorPropertyTypes.Username ||
                x.Type == NotifcationActorDescription.ActorPropertyTypes.Password 
                ).WithMessage(localizer["ValidationNotEmpty"]);

                element.RuleFor(x => x.Value).Must(x => {
                    try
                    {
                        var uri = new Uri(x, UriKind.Absolute);
                        return  uri.Scheme == "http" || uri.Scheme == "https";
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }).WithMessage(localizer["ValidationNotAValidUrl"]).When(x => x.Type == NotifcationActorDescription.ActorPropertyTypes.Endpoint);

                element.RuleFor(x => x.Values).Cascade(CascadeMode.Stop).NotNull().NotEmpty().Must(x => x.Any(y => y.IsSelected == true)).WithMessage(localizer["ValidationAtLeastOneScopeSelected"]).When(x => x.Type == NotifcationActorDescription.ActorPropertyTypes.DHCPv6ScopeList);

            }
           );
        }
    }
}
