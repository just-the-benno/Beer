using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.NotificationPipelineRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.NotificationPipelineResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public partial class CreateNotificationPipelinePage : IDisposable
    {
        private MudTabs _tabs;

        private CreateNotificationPipelineGenerellPropertiesViewModel _generellPropertiesModel;
        private CreateNotificationPipelineTriggerPropertiesViewModel _triggerPropertiesModel;
        private CreateNotificationPipelineConditionPropertiesViewModel _conditionPropertiesModel;
        private CreateNotificationPipelineActorPropertiesViewModel _actorPropertiesModel;

        private EditContext _generellPropertiesContext;
        private EditContext _triggerContext;
        private EditContext _conditionContext;
        private EditContext _actorContext;

        private Boolean _submitInProgress;

        private Boolean _serviceErrorOccured = false;
        private NotificationPipelineDescriptions _pipelineDescriptions;
        private IList<DHCPv6ScopeItem> _scopes;

        public IEnumerable<String> PossibleCondtions { get; private set; } = new List<String>();
        public IEnumerable<String> PossibleActors { get; private set; } = new List<String>();

        public CreateNotificationPipelinePage()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CreateContexts();
        }

        private void CreateContexts()
        {
            _generellPropertiesModel = new();
            _triggerPropertiesModel = new();
            _conditionPropertiesModel = new();
            _actorPropertiesModel = new();

            _generellPropertiesContext = new EditContext(_generellPropertiesModel);
            _triggerContext = new EditContext(_triggerPropertiesModel);
            _conditionContext = new EditContext(_conditionPropertiesModel);
            _actorContext = new EditContext(_actorPropertiesModel);

            _generellPropertiesContext.OnFieldChanged += GenerellPropertiesContextOnFieldChanged;
            _triggerContext.OnFieldChanged += TriggerContextOnFieldChanged;
            _conditionContext.OnFieldChanged += ConditionContextOnFieldChanged;
            _actorContext.OnFieldChanged += ActorContextOnFieldChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _pipelineDescriptions = await _service.GetpipelineDescriptions();
            _conditionPropertiesModel.SetDescription(_pipelineDescriptions);
            _actorPropertiesModel.SetDescription(_pipelineDescriptions);
        }

        private void ValidateContext(EditContext context)
        {
            context.Validate();
            StateHasChanged();
        }

        private async void ActorContextOnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            ValidateContext(_actorContext);

            if (e.FieldIdentifier.FieldName == nameof(CreateNotificationPipelineActorPropertiesViewModel.ActorName))
            {
                if (_scopes == null &&
                    _actorPropertiesModel.Entries.Count(x => x.Type ==  NotifcationActorDescription.ActorPropertyTypes.DHCPv6ScopeList) > 0)
                {
                    _scopes = (await _service.GetDHCPv6ScopesAsList()).ToList();
                }

                _actorPropertiesModel.SetScopes(_scopes);
                base.StateHasChanged();
            }
        }

        private async void ConditionContextOnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            ValidateContext(_conditionContext);

            if (e.FieldIdentifier.FieldName == nameof(CreateNotificationPipelineConditionPropertiesViewModel.ConditionName))
            {
                if (_scopes == null &&
                    _conditionPropertiesModel.Entries.Count(x => x.Type == NotificationCondititonDescription.ConditionsPropertyTypes.DHCPv6ScopeList) > 0)
                {
                    _scopes = (await _service.GetDHCPv6ScopesAsList()).ToList();
                }

                _conditionPropertiesModel.SetScopes(_scopes);
                base.StateHasChanged();
            }
        }

        private void TriggerContextOnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            ValidateContext(_conditionContext);

            if (e.FieldIdentifier.FieldName == nameof(CreateNotificationPipelineTriggerPropertiesViewModel.TriggerName))
            {
                if (String.IsNullOrEmpty(_triggerPropertiesModel.TriggerName) == false)
                {
                    var mapperEntry = _pipelineDescriptions.MapperEnries.First(x => x.TriggerName == _triggerPropertiesModel.TriggerName);
                    PossibleCondtions = _pipelineDescriptions.Conditions.Where(x => mapperEntry.CompactibleConditions.Contains(x.Name)).Select(x => x.Name).ToList();
                    PossibleActors = _pipelineDescriptions.Actors.Where(x => mapperEntry.CompactibleActors.Contains(x.Name)).Select(x => x.Name).ToList();
                }
                else
                {
                    PossibleCondtions = Array.Empty<String>();
                    PossibleActors = Array.Empty<String>();
                }
            }
        }

        private void GenerellPropertiesContextOnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            ValidateContext(_generellPropertiesContext);
        }

        public void Dispose()
        {
            _generellPropertiesContext.OnFieldChanged -= GenerellPropertiesContextOnFieldChanged;
            _triggerContext.OnFieldChanged -= TriggerContextOnFieldChanged;
            _conditionContext.OnFieldChanged -= ConditionContextOnFieldChanged;
            _actorContext.OnFieldChanged -= ActorContextOnFieldChanged;
        }

        private void NavigateToStep(Int32 step) => _tabs.ActivatePanel(step - 1);

        public CreateNotifcationPipelineRequest GetRequest() => new()
        {
            Name = _generellPropertiesModel.Name,
            Description = _generellPropertiesModel.Description,
            TriggerName = _triggerPropertiesModel.TriggerName,
            CondtionName = _conditionPropertiesModel.ConditionName,
            ConditionProperties = _conditionPropertiesModel.Entries.ToDictionary(x => x.Name, x => x.GetSerializedValues()),
            ActorName = _actorPropertiesModel.ActorName,
            ActorProperties = _actorPropertiesModel.Entries.ToDictionary(x => x.Name, x => x.GetSerializedValues())
        };

        public async Task CreatePipeline()
        {
            var request = GetRequest();
            _serviceErrorOccured = false;
            _submitInProgress = true;

            Boolean serviceResult = await _service.CreateNotificationPipeline(request);
            _serviceErrorOccured = !serviceResult;
            _submitInProgress = false;
            if (serviceResult == true)
            {
                snackBarService.Add(String.Format(L["CreatePipelineSuccessMessage"], request.Name), Severity.Success);
                _navigator.NavigateTo("/pipelines");
            }
        }
    }
}
