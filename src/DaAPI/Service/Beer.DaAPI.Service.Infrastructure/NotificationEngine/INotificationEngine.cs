using Beer.DaAPI.Core.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Beer.DaAPI.Infrastructure.NotificationEngine.NotifciationsReadModels.V1;

namespace Beer.DaAPI.Infrastructure.NotificationEngine
{
    public interface INotificationEngine
    {
        Task Initialize();

        Task<IEnumerable<NotificationPipelineReadModel>> GetPipelines();
        Task<NotificationPipelineDescriptions> GetPiplelineDescriptions();
        Task<Boolean> AddNotificationPipeline(NotificationPipeline pipeline);
        Task<Boolean> DeletePipeline(Guid id);
        Task HandleTrigger(NotifcationTrigger trigger);
        Int32 GetPipelineAmount();
    }
}
