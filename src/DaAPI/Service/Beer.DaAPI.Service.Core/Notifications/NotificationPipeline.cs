using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Tracing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Beer.DaAPI.Core.Notifications.NotificationsEvent.V1;

namespace Beer.DaAPI.Core.Notifications
{
    public enum NotifactionPipelineExecutionResults
    {
        TriggerNotMatch = 1,
        ConditionNotMatched = 2,
        ActorFailed = 3,
        Success = 5,
    }

    public class NotificationPipeline : AggregateRootWithEvents, ITracingRecord
    {
        private const Int32 _startExecutionTracingNumber = 1;
        private const Int32 _checkingNotDefaultConditionTracingNumber = 2;
        private const Int32 _defaultConditionTracingNumber = 3;
        private const Int32 _startActorTracingNumber = 4;
        private const Int32 _actorFailedTracingNumber = 5;
        private const Int32 _actorFailedWithErrorTracingNumber = 10;

        private readonly ILogger<NotificationPipeline> _logger;
        private readonly INotificationConditionFactory _conditionFactory;
        private readonly INotificationActorFactory _actorFactory;

        #region Properties

        public NotificationPipelineName Name { get; private set; }
        public NotificationPipelineDescription Description { get; private set; }
        public String TriggerIdentifier { get; private set; }
        public NotificationCondition Condition { get; private set; }
        public NotificationActor Actor { get; private set; }

        Guid? ITracingRecord.Id => base.Id;

        #endregion

        #region Constructor and factories

        public NotificationPipeline(Guid id,
            INotificationConditionFactory conditionFactory, INotificationActorFactory actorFactory, ILogger<NotificationPipeline> logger
            ) : base(id)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _conditionFactory = conditionFactory;
            _actorFactory = actorFactory;
        }

        public bool CanExecute(NotifcationTrigger trigger) => trigger.GetTypeIdentifier() == TriggerIdentifier;

        public static NotificationPipeline Create(NotificationPipelineName name, NotificationPipelineDescription description,
            String triggerTypeIdentifier, NotificationCondition conndition, NotificationActor actor,
            ILogger<NotificationPipeline> logger, INotificationConditionFactory conditionFactory, INotificationActorFactory actorFactory)
        {
            if (conndition == null)
            {
                conndition = NotificationCondition.True;
            }
            Guid id = Guid.NewGuid();

            var pipeline = new NotificationPipeline(id, conditionFactory, actorFactory, logger);
            pipeline.Apply(new NotificationPipelineCreatedEvent
            {
                Id = id,
                Name = name,
                Description = description,
                TriggerIdentifier = triggerTypeIdentifier,
                ConditionCreateInfo = conndition.ToCreateModel(),
                ActorCreateInfo = actor.ToCreateModel(),
            });

            return pipeline;
        }

        #endregion

        #region Methods

        public async Task<NotifactionPipelineExecutionResults> Execute(NotifcationTrigger trigger, TracingStream tracingStream)
        {
            await tracingStream.Append(_startExecutionTracingNumber, TracingRecordStatus.Informative, this);
            _logger.LogDebug("start of {name} pipeline", Name);

            if (TriggerIdentifier != trigger.GetTypeIdentifier())
            {
                _logger.LogDebug("type mismatch. Expected a trigger of {type} but received {otherType}", TriggerIdentifier, trigger.GetTypeIdentifier());
                return NotifactionPipelineExecutionResults.TriggerNotMatch;
            }

            if (Condition != NotificationCondition.True)
            {
                tracingStream.OpenNextLevel(_checkingNotDefaultConditionTracingNumber);
                tracingStream.OpenNextLevel(Condition.GetTracingIdentifier());

                if (await Condition.IsValid(trigger, tracingStream) == false)
                {
                    _logger.LogDebug("the trigger doens't satisfy the condition. Execution of pipeline stopped");
                    return NotifactionPipelineExecutionResults.ConditionNotMatched;
                }
                else
                {
                    _logger.LogDebug("the trigger {trgger} meet the condtion {condition}", trigger, Condition);
                }

                tracingStream.RevertLevel();
                tracingStream.RevertLevel();
            }
            else
            {
                await tracingStream.Append(_defaultConditionTracingNumber, TracingRecordStatus.Informative, this);

                _logger.LogDebug("no conditions applied. actor enabled");
            }

            _logger.LogDebug("executing actor...");

            Int32 expectedLevel = tracingStream.GetCurrentLevel();

            tracingStream.OpenNextLevel(_startActorTracingNumber);
            tracingStream.OpenNextLevel(Actor.GetTracingIdentifier());

            try
            {
                Boolean actorResult = await Actor.Handle(trigger, tracingStream);
                if (actorResult == false)
                {
                    tracingStream.RevertToLevel(expectedLevel);

                    await tracingStream.Append(_actorFailedTracingNumber, TracingRecordStatus.Error, new Dictionary<String, String>());
                    _logger.LogError("Actor {actor} of pipeline {name} failed.", Actor, Name);
                    return NotifactionPipelineExecutionResults.ActorFailed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to execute actor");
                tracingStream.RevertToLevel(expectedLevel);

                await tracingStream.Append(_actorFailedWithErrorTracingNumber, TracingRecordStatus.Error, new Dictionary<String, String> {
                    {  "ErrorType", ex.GetType().Name },
                    {  "ErrorMessage", ex.Message },
                    {  "ErrorDetails", ex.ToString() },
                    });

                return NotifactionPipelineExecutionResults.ActorFailed;
            }

            tracingStream.RevertToLevel(expectedLevel);

            return NotifactionPipelineExecutionResults.Success;

        }

        #endregion

        #region When


        protected override void When(DomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case NotificationPipelineCreatedEvent e:
                    Name = new NotificationPipelineName(e.Name);
                    Description = new NotificationPipelineDescription(e.Description);
                    Id = e.Id;
                    TriggerIdentifier = e.TriggerIdentifier;
                    Condition = _conditionFactory.Initilize(e.ConditionCreateInfo);
                    Actor = _actorFactory.Initilize(e.ActorCreateInfo);
                    break;
                default:
                    break;
            }
        }

        #endregion

        public override void PrepareForDelete()
        {
            base.Apply(new NotificationPipelineDeletedEvent(Id));
        }

        public IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<String, String>
        {
            { "Id", Id.ToString() },
            { "Name", Name.Value },
            { "Trigger", TriggerIdentifier },
            { "Condition", JsonSerializer.Serialize(Condition.GetTracingRecordDetails()) },
            { "Actor", JsonSerializer.Serialize(Actor.GetTracingRecordDetails()) },
        };

        public Boolean HasIdentity() => true;
    }
}
