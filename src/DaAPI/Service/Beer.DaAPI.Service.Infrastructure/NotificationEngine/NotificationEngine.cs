using Beer.DaAPI.Core.Notifications;
using Beer.DaAPI.Core.Notifications.Actors;
using Beer.DaAPI.Core.Notifications.Conditions;
using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Tracing;
using Beer.DaAPI.Infrastructure.StorageEngine.DHCPv6;
using Beer.DaAPI.Infrastructure.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Infrastructure.NotificationEngine.NotifciationsReadModels.V1;

namespace Beer.DaAPI.Infrastructure.NotificationEngine
{
    public class NotificationEngine : INotificationEngine
    {

        private readonly IDHCPv6StorageEngine _storageEngine;
        private readonly ITracingManager _tracingManager;
        private List<NotificationPipeline> _pipelines;

        public NotificationEngine(IDHCPv6StorageEngine storageEngine, ITracingManager tracingManager)
        {
            this._storageEngine = storageEngine;
            this._tracingManager = tracingManager;
        }

        public async Task Initialize()
        {
            _pipelines = new List<NotificationPipeline>(await _storageEngine.GetAllNotificationPipeleines());
        }

        public async Task<Boolean> AddNotificationPipeline(NotificationPipeline pipeline)
        {
            Boolean result = await _storageEngine.Save(pipeline);
            if (result == true)
            {
                _pipelines.Add(pipeline);
            }

            return result;
        }

        public async Task<Boolean> DeletePipeline(Guid id)
        {
            var pipeline = _pipelines.FirstOrDefault(x => x.Id == id);
            if (pipeline == null)
            {
                return false;
            }

            if (await _storageEngine.DeleteAggregateRoot(pipeline) == false)
            {
                return false;
            }

            _pipelines.Remove(pipeline);
            return true;
        }

        public async Task HandleTrigger(NotifcationTrigger trigger)
        {
            var tracingStream = await _tracingManager.NewTrace(TracingManagerConstants.Modules.NotificationEngine, TracingManagerConstants.NotifcationEngineSubModels.HandleTriggerStarted,
                trigger);

            foreach (var item in _pipelines)
            {
                tracingStream.SetEntityId(item.Id);
                await tracingStream.Append(TracingManagerConstants.NotifcationEngineSubModels.HandleTriggerStarted, item);

                if (item.CanExecute(trigger) == true)
                {
                    await tracingStream.Append(TracingManagerConstants.NotifcationEngineSubModels.PipelineCanHandleTrigger, item);
                    tracingStream.OpenNextLevel(TracingManagerConstants.NotifcationEngineSubModels.ExecutionPipelineStarted);

                    await item.Execute(trigger, tracingStream);

                    tracingStream.RevertLevel();
                }
                else
                {
                    await tracingStream.Append(TracingManagerConstants.NotifcationEngineSubModels.PipelineCanNotHandleTrigger, item);
                }

                tracingStream.ClearEntity();
            }

            await tracingStream.AppendAndClose(TracingManagerConstants.NotifcationEngineSubModels.TriggerHandled, trigger);

        }

        public Task<IEnumerable<NotificationPipelineReadModel>> GetPipelines()
        {
            var result = _pipelines.Select(x => new NotificationPipelineReadModel
            {
                Id = x.Id,
                Name = x.Name,
                TrigerName = x.TriggerIdentifier,
                ActorName = x.Actor.GetType().Name,
                ConditionName = x.Condition == null ? "None" : x.Condition.GetType().Name,
            }).ToList();

            return Task.FromResult<IEnumerable<NotificationPipelineReadModel>>(result);
        }

        public Int32 GetPipelineAmount() => _pipelines.Count();

        public Task<NotificationPipelineDescriptions> GetPiplelineDescriptions()
        {
            var triggerDescriptions = new[]
            {
                new NotificationTriggerDescription
                {
                    Name = nameof(PrefixEdgeRouterBindingUpdatedTrigger),
                }
            };

            var conditionsDescriptions = new[]
            {
                new NotifcationCondititonDescription
                {
                    Name = nameof(DHCPv6ScopeIdNotificationCondition),
                    Properties = new Dictionary<string, NotifcationCondititonDescription.ConditionsPropertyTtpes>
                    {
                        { nameof(DHCPv6ScopeIdNotificationCondition.IncludesChildren), NotifcationCondititonDescription.ConditionsPropertyTtpes.Boolean  },
                        { nameof(DHCPv6ScopeIdNotificationCondition.ScopeIds), NotifcationCondititonDescription.ConditionsPropertyTtpes.DHCPv6ScopeList  },
                    }
                },
            };

            var actorDescription = new[]
            {
                new NotifcationActorDescription
                {
                    Name = nameof(NxOsStaticRouteUpdaterNotificationActor),
                    Properties = new Dictionary<String,NotifcationActorDescription.ActorPropertyTtpes>
                    {
                        { nameof(NxOsStaticRouteUpdaterNotificationActor.Url),  NotifcationActorDescription.ActorPropertyTtpes.Endpoint  },
                        { nameof(NxOsStaticRouteUpdaterNotificationActor.Username),  NotifcationActorDescription.ActorPropertyTtpes.Username  },
                        { nameof(NxOsStaticRouteUpdaterNotificationActor.Password),  NotifcationActorDescription.ActorPropertyTtpes.Password  },
                    }
                }
            };

            var mapping = new[]
            {
                new NotificationPipelineTriggerMapperEntry
                {
                 TriggerName = nameof(PrefixEdgeRouterBindingUpdatedTrigger),
                 CompactibleConditions = new[] { nameof(DHCPv6ScopeIdNotificationCondition) },
                 CompactibleActors = new[] { nameof(NxOsStaticRouteUpdaterNotificationActor) }
                }
            };

            NotificationPipelineDescriptions result = new NotificationPipelineDescriptions
            {
                Actors = actorDescription,
                Conditions = conditionsDescriptions,
                Trigger = triggerDescriptions,
                MapperEnries = mapping,
            };

            return Task.FromResult(result);
        }
    }
}
